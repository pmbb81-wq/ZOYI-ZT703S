using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZOYI
{
    public partial class MainWindow : Form
    {
        String? COMportName = "";
        PARSE_MODE COMparseMode = PARSE_MODE.EXT;
        CancellationTokenSource ctsReadCOM;
        private Dictionary<string, string> _portAliases = new();
        private const string PORT_ALIASES_KEY = "com_port_aliases";
        private volatile bool _dataReceivedAfterConnect = false;
        private System.Windows.Forms.Timer? _autoReconnectTimer;
        private volatile bool _autoReconnecting = false;
        private static List<EsrCsvRow> _bazaEsr = null;
        private static readonly object _bazaLock = new();
        private double? _customTanDelta = null;

        private class EsrCsvRow
        {
            public double PojemnoscUf { get; set; }
            public double NapiecieV { get; set; }
            public double TemperaturaC { get; set; }
            public double MaxTanDelta { get; set; }
        }

        /*
         * 
         */
        private void rbComParse_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCOMparseStd.Checked)
                COMparseMode = PARSE_MODE.STD;
            else if (rbCOMparseExt.Checked)
                COMparseMode = PARSE_MODE.EXT;
            else if (rbCOMparseLua.Checked)
            {
                COMparseMode = PARSE_MODE.LUA;
                frame_dec.LuaReload(luaPath);
            }
            else
                COMparseMode = PARSE_MODE.RAW;
        }

        /*
         * 
         */
        private async void btnCOMluaEdit_Click(object sender, EventArgs e)
        {
            RichEditor re = new RichEditor(frame_dec);
            re.loadFile(luaPath, RichEditor.ENCODING.LUA);

            string result = await re.AsyncEdit();

            if (result == "save")
                mLua.LuaReload(luaPath);
        }

        /*
         * 
         */
        private void btnListCOM_Click(object sender, EventArgs e)
        {
            refreshCOMlist();
        }

        /*
         * 
         */
        private void btnComConnect_Click(object sender, EventArgs e)
        {
            if (!comx.isConnected())
            {
                try
                {
                    COMportName = ResolvePortName(lbListCOMs.SelectedItem);
                    int baudrate = int.TryParse(tbCOMBaudrate.Text, out var b1) ? b1 : 115200;

                    comx.connect(COMportName!, baudrate);

                    StopAutoReconnect();

                    LocalSettings.LastComPort = COMportName!;

                    btnComConnect.Text = "ROZŁĄCZ " + COMportName;
                    btnComConnect.BackColor = Color.LightCoral;
                    lbListCOMs.Enabled = false;
                    tbCOMBaudrate.Enabled = false;
                    lblComConnStatus.Text = "POŁĄCZONY";
                    lblComConnStatus.ForeColor = Color.LightCoral;

                    ctsReadCOM = new CancellationTokenSource();
                    var task = Task.Run(() => readCom(ctsReadCOM.Token));
                    Console.WriteLine("Task readCom run...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show(
                        $"Nie udało się połączyć z urządzeniem na porcie {COMportName}.\n\n{ex.Message}",
                        "Błąd połączenia",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                btnComConnect.Text = "POŁĄCZ";
                btnComConnect.BackColor = Color.LightGreen;
                lblComConnStatus.Text = "ROZŁĄCZONY";
                lblComConnStatus.ForeColor = Color.LightGreen;
                lbListCOMs.Enabled = true;
                tbCOMBaudrate.Enabled = true;
                ctsReadCOM.Cancel();
                ctsReadCOM.Dispose();
                comx.disconnect();
                StopAutoReconnect();
            }
        }

        /*
         * 
         */
        private void ApplyConnectedUI(string portName)
        {
            btnComConnect.Text = "ROZŁĄCZ " + portName;
            btnComConnect.BackColor = Color.LightCoral;
            lbListCOMs.Enabled = false;
            tbCOMBaudrate.Enabled = false;
            lblComConnStatus.Text = "POŁĄCZONY";
            lblComConnStatus.ForeColor = Color.LightCoral;
        }

        /*
         * 
         */
        private void ApplyDisconnectedUI()
        {
            btnComConnect.Text = "POŁĄCZ";
            btnComConnect.BackColor = Color.LightGreen;
            lblComConnStatus.Text = "ROZŁĄCZONY";
            lblComConnStatus.ForeColor = Color.LightGreen;
            lbListCOMs.Enabled = true;
            tbCOMBaudrate.Enabled = true;
        }

        async Task readCom(CancellationToken token)
        {
            String buff = "";
            byte[] bytesArray = new byte[18];
            int indexBytesArray = 0;
            int consecutiveTimeouts = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        int readByte = await comx.readByteAsync();

                        if (readByte == -2)
                        {
                            consecutiveTimeouts++;
                            if (consecutiveTimeouts >= 10)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    ApplyDisconnectedUI();
                                    MessageBox.Show(
                                        $"Urządzenie na porcie {COMportName} nie odpowiada.\n\nSprawdź czy urządzenie jest włączone i w zasięgu Bluetooth.",
                                        "Urządzenie niedostępne",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                    lblComConnStatus.Text = "AUTO-CONNECT...";
                                    lblComConnStatus.ForeColor = Color.Yellow;
                                    StartAutoReconnect();
                                }));
                                comx.disconnect();
                                return;
                            }
                            continue;
                        }

                        consecutiveTimeouts = 0;

                        if (readByte == -1)
                        {
                            if (!comx.isConnected())
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    ApplyDisconnectedUI();
                                    MessageBox.Show(
                                        $"Urządzenie na porcie {COMportName} zostało rozłączone.",
                                        "Urządzenie niedostępne",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                    lblComConnStatus.Text = "AUTO-CONNECT...";
                                    lblComConnStatus.ForeColor = Color.Yellow;
                                    StartAutoReconnect();
                                }));
                                return;
                            }
                            continue;
                        }

                        char c = (char)readByte;
                        _dataReceivedAfterConnect = true;

                        // RAW
                        if (COMparseMode == PARSE_MODE.RAW)
                        {
                            tbComOutput.Invoke(new Action(() =>
                            {
                                tbComOutput.AppendText(c.ToString());
                            }));
                        }
                        // EXTENDED
                        else if (COMparseMode == PARSE_MODE.EXT)
                        {
                            if ((byte)readByte == 0xFF)
                            {
                                indexBytesArray = 0;
                                bytesArray[indexBytesArray++] = (byte)readByte;
                            }
                            while (indexBytesArray < 18)
                            {
                                Console.WriteLine(indexBytesArray);
                                bytesArray[indexBytesArray++] = (byte)await comx.readByteAsync();
                            }
                            frame_dec.DecodeExtended(bytesArray);

                            tbComOutput.Invoke(new Action(() =>
                            {
                                tbComOutput.AppendText(
                                    $"{frame_dec.Value} {frame_dec.Unit} {frame_dec.Mode2} {frame_dec.Freq} {frame_dec.Mode1}");
                                tbComOutput.AppendText(Environment.NewLine);
                            }));

                            try
                            {
                                standardDisplayPanel.Invoke(new Action(() =>
                                {
                                    standardDisplayPanel.updatePanel(frame_dec);
                                }));

                                advancedDisplayPanel.Invoke(new Action(() =>
                                {
                                    advancedDisplayPanel.updatePanel(frame_dec);
                                }));

                                gaugeOverlayPanel.Invoke(new Action(() =>
                                {
                                    gaugeOverlayPanel.updatePanel(frame_dec);
                                }));
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("readCom panel update Exception");
                            }

                            try
                            {
                                buzzerManager.CheckAlarm(frame_dec.Value?.Trim());
                                buzzerManager.CheckContinuityBeep(frame_dec);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("readCom buzzer Exception");
                            }

                            try
                            {
                                chartPanel.Invoke(new Action(() =>
                                {
                                    chartPanel.AddDataPoint(frame_dec);
                                }));
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("readCom chart update Exception");
                            }

                            SpeakMeasurement(frame_dec.Value?.Trim(), frame_dec.Unit);

                            lock (csvLock)
                            {
                                csvData.Add(new string[]
                                {
                                    DateTime.Now.ToString("HH:mm:ss.fff"),
                                    frame_dec.Value?.Trim() ?? "",
                                    frame_dec.Unit ?? "",
                                    frame_dec.Mode2 ?? "",
                                    frame_dec.Mode1 ?? "",
                                    frame_dec.Freq ?? "",
                                    frame_dec.Freq_unit ?? "",
                                    ""
                                });
                            }
                        }
                        // STANDARD/LUA
                        else
                        {
                            buff += c;

                            if (c == ' ')
                            {
                                if (COMparseMode == PARSE_MODE.STD)
                                    frame_dec.DecodeStdandard(buff);
                                else if (COMparseMode == PARSE_MODE.LUA)
                                    frame_dec.DecodeLua(buff);

                                buff = "";

                                tbComOutput.Invoke(new Action(() =>
                                {
                                    tbComOutput.AppendText($"{frame_dec.Label} : {frame_dec.Value} {frame_dec.Unit}");
                                    tbComOutput.AppendText(Environment.NewLine);
                                }));

                                // wyjątek kiedy panel ukryty
                                // dodać bool panel visible
                                try
                                {
                                    standardDisplayPanel.Invoke(new Action(() =>
                                    {
                                        standardDisplayPanel.updatePanel(frame_dec);
                                    }));

                                    advancedDisplayPanel.Invoke(new Action(() =>
                                    {
                                        advancedDisplayPanel.updatePanel(frame_dec);
                                    }));

                                    gaugeOverlayPanel.Invoke(new Action(() =>
                                    {
                                        gaugeOverlayPanel.updatePanel(frame_dec);
                                    }));

                                    chartPanel.Invoke(new Action(() =>
                                    {
                                        chartPanel.AddDataPoint(frame_dec);
                                    }));
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("readCom Exception");
                                }

                                SpeakMeasurement(frame_dec.Value?.Trim(), frame_dec.Unit);

                                try
                                {
                                    buzzerManager.CheckAlarm(frame_dec.Value?.Trim());
                                    buzzerManager.CheckContinuityBeep(frame_dec);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("readCom buzzer Exception");
                                }

                                lock (csvLock)
                                {
                                    csvData.Add(new string[]
                                    {
                                        DateTime.Now.ToString("HH:mm:ss.fff"),
                                        frame_dec.Value?.Trim() ?? "",
                                        frame_dec.Unit ?? "",
                                        frame_dec.Mode2 ?? "",
                                        frame_dec.Mode1 ?? "",
                                        frame_dec.Freq ?? "",
                                        frame_dec.Freq_unit ?? "",
                                        ""
                                    });
                                }

                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeoutException");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("OperationCanceledException");
            }

            Console.WriteLine("Task readCom finish");
        }

        /*
         * 
         */
        private Dictionary<string, string> GetPortDeviceNames()
        {
            var result = new Dictionary<string, string>();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Caption, DeviceID FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string caption = obj["Caption"]?.ToString() ?? "";
                    string deviceId = obj["DeviceID"]?.ToString() ?? "";

                    var match = System.Text.RegularExpressions.Regex.Match(caption, @"\((COM\d+)\)");
                    if (!match.Success) continue;
                    string portName = match.Groups[1].Value;
                    string deviceName = caption.Replace($"({portName})", "").Trim();

                    if (deviceId.StartsWith("BTHENUM\\"))
                    {
                        string? btName = GetBluetoothDeviceName(deviceId);
                        if (!string.IsNullOrEmpty(btName))
                            deviceName = btName;
                    }

                    result.TryAdd(portName, deviceName);
                }
            }
            catch { }
            return result;
        }

        private static string? GetBluetoothDeviceName(string deviceId)
        {
            try
            {
                var macMatch = System.Text.RegularExpressions.Regex.Match(
                    deviceId, @"([0-9A-Fa-f]{12})_[0-9A-Fa-f]+$");
                if (!macMatch.Success) return null;

                string mac = macMatch.Groups[1].Value.ToLowerInvariant();
                if (mac == "000000000000") return null;

                string regPath = $@"SYSTEM\CurrentControlSet\Services\BTHPORT\Parameters\Devices\{mac}";
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath);
                if (key == null) return null;

                var nameValue = key.GetValue("Name");
                if (nameValue is byte[] nameBytes && nameBytes.Length > 0)
                    return Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');

                return null;
            }
            catch { return null; }
        }

        void refreshCOMlist()
        {
            var ports = comx.listCOMports();
            var deviceNames = GetPortDeviceNames();
            lbListCOMs.Items.Clear();
            foreach (var port in ports)
            {
                if (_portAliases.TryGetValue(port, out var alias) && !string.IsNullOrWhiteSpace(alias))
                    lbListCOMs.Items.Add($"{port} — {alias}");
                else if (deviceNames.TryGetValue(port, out var devName) && !string.IsNullOrWhiteSpace(devName))
                    lbListCOMs.Items.Add($"{port} — {devName}");
                else
                    lbListCOMs.Items.Add(port);
            }
        }

        private string? ResolvePortName(object? item)
        {
            if (item == null) return null;
            string text = item.ToString()!;
            if (text.Contains(" — "))
                return text.Split(new[] { " — " }, 2, StringSplitOptions.None)[0];
            return text;
        }

        private void LoadPortAliases()
        {
            try
            {
                string json = Properties.Settings.Default.com_port_aliases ?? "";
                if (!string.IsNullOrWhiteSpace(json))
                    _portAliases = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
            catch { _portAliases = new(); }
        }

        private void SavePortAliases()
        {
            try
            {
                var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                Properties.Settings.Default.com_port_aliases = System.Text.Json.JsonSerializer.Serialize(_portAliases, opts);
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void InitPortContextMenu()
        {
            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Nazwij port...", null, (_, _) => RenameSelectedPort());
            ctx.Items.Add("Przywróć domyślną nazwę", null, (_, _) => ResetSelectedPortName());
            lbListCOMs.ContextMenuStrip = ctx;
        }

        private void RenameSelectedPort()
        {
            if (lbListCOMs.SelectedItem == null) return;
            string portName = ResolvePortName(lbListCOMs.SelectedItem)!;
            string currentAlias = _portAliases.TryGetValue(portName, out var a) ? a : "";

            using var dlg = new Form
            {
                Text = $"Nazwa portu: {portName}",
                Size = new Size(350, 130),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var lbl = new Label { Text = "Własna nazwa:", ForeColor = Color.LightGray, Location = new Point(10, 10), AutoSize = true };
            var tb = new TextBox { Text = currentAlias, Location = new Point(10, 35), Width = 310, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(10, 68), Width = 80, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Location = new Point(100, 68), Width = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            dlg.Controls.AddRange(new Control[] { lbl, tb, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string alias = tb.Text.Trim();
                if (string.IsNullOrWhiteSpace(alias))
                    _portAliases.Remove(portName);
                else
                    _portAliases[portName] = alias;
                SavePortAliases();
                int idx = lbListCOMs.SelectedIndex;
                refreshCOMlist();
                if (idx >= 0 && idx < lbListCOMs.Items.Count)
                    lbListCOMs.SelectedIndex = idx;
            }
        }

        private void ResetSelectedPortName()
        {
            if (lbListCOMs.SelectedItem == null) return;
            string portName = ResolvePortName(lbListCOMs.SelectedItem)!;
            if (_portAliases.Remove(portName))
            {
                SavePortAliases();
                int idx = lbListCOMs.SelectedIndex;
                refreshCOMlist();
                if (idx >= 0 && idx < lbListCOMs.Items.Count)
                    lbListCOMs.SelectedIndex = idx;
            }
        }

        /*
         * 
         */
        private void chkAutoConnect_CheckedChanged(object? sender, EventArgs e)
        {
            LocalSettings.AutoConnect = chkAutoConnect.Checked;
            if (!chkAutoConnect.Checked)
                StopAutoReconnect();
        }

        /*
         * 
         */
        internal void LoadAutoConnectState()
        {
            chkAutoConnect.Checked = LocalSettings.AutoConnect;
        }

        /*
         * 
         */
        internal void TryAutoConnect()
        {
            if (!LocalSettings.AutoConnect) return;

            if (comx.isConnected()) return;

            string lastPort = LocalSettings.LastComPort;
            if (string.IsNullOrWhiteSpace(lastPort)) return;

            refreshCOMlist();

            string? displayName = null;
            foreach (var item in lbListCOMs.Items)
            {
                if (ResolvePortName(item) == lastPort)
                {
                    displayName = item.ToString();
                    break;
                }
            }

            if (displayName == null)
            {
                lblComConnStatus.Text = "AUTO-CONNECT...";
                lblComConnStatus.ForeColor = Color.Yellow;
                StartAutoReconnect();
                return;
            }

            lbListCOMs.SelectedItem = displayName;

            try
            {
                COMportName = lastPort;
                int baudrate = int.TryParse(tbCOMBaudrate.Text, out var b2) ? b2 : 115200;

                _dataReceivedAfterConnect = false;
                comx.connect(COMportName!, baudrate);

                ApplyConnectedUI(COMportName!);

                ctsReadCOM = new CancellationTokenSource();
                var task = Task.Run(() => readCom(ctsReadCOM.Token));

                _ = WaitForDataAsync(3000, COMportName!);
                Console.WriteLine("AutoConnect: Task readCom run...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoConnect failed: " + ex.Message);
                ApplyDisconnectedUI();
                lblComConnStatus.Text = "AUTO-CONNECT...";
                lblComConnStatus.ForeColor = Color.Yellow;
                StartAutoReconnect();
            }
        }

        /*
         * 
         */
        private async Task WaitForDataAsync(int timeoutMs, string portName)
        {
            await Task.Delay(timeoutMs);

            if (_dataReceivedAfterConnect) return;
            if (!comx.isConnected()) return;

            try
            {
                ctsReadCOM?.Cancel();
                comx.disconnect();

                this.BeginInvoke(new Action(() =>
                {
                    ApplyDisconnectedUI();
                    MessageBox.Show(
                        $"Urządzenie na porcie {portName} nie odpowiada.\n\nSprawdź czy urządzenie jest włączone i w zasięgu Bluetooth.",
                        "Auto Connect — brak odpowiedzi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    lblComConnStatus.Text = "AUTO-CONNECT...";
                    lblComConnStatus.ForeColor = Color.Yellow;
                    StartAutoReconnect();
                }));
            }
            catch { }
        }

        /*
         * 
         */
        private void StartAutoReconnect()
        {
            if (!LocalSettings.AutoConnect) return;
            if (_autoReconnectTimer != null) return;

            _autoReconnectTimer = new System.Windows.Forms.Timer();
            _autoReconnectTimer.Interval = 1000;
            _autoReconnectTimer.Tick += AutoReconnectTick;
            _autoReconnectTimer.Start();
            Console.WriteLine("AutoReconnect: started");
        }

        /*
         * 
         */
        private void StopAutoReconnect()
        {
            if (_autoReconnectTimer != null)
            {
                _autoReconnectTimer.Stop();
                _autoReconnectTimer.Dispose();
                _autoReconnectTimer = null;
                Console.WriteLine("AutoReconnect: stopped");
            }
        }

        /*
         * 
         */
        private void AutoReconnectTick(object? sender, EventArgs e)
        {
            if (comx.isConnected())
            {
                StopAutoReconnect();
                return;
            }

            if (_autoReconnecting) return;

            string lastPort = LocalSettings.LastComPort;
            if (string.IsNullOrWhiteSpace(lastPort))
            {
                StopAutoReconnect();
                return;
            }

            var ports = comx.listCOMports();
            if (!ports.Contains(lastPort))
            {
                this.BeginInvoke(new Action(() => refreshCOMlist()));
                return;
            }

            _autoReconnecting = true;
            int baudrate = int.TryParse(tbCOMBaudrate.Text, out var br) ? br : 115200;

            Task.Run(() =>
            {
                try
                {
                    _dataReceivedAfterConnect = false;
                    comx.connect(lastPort, baudrate);

                    this.BeginInvoke(new Action(() =>
                    {
                        COMportName = lastPort;
                        StopAutoReconnect();
                        ApplyConnectedUI(lastPort);

                        LocalSettings.LastComPort = lastPort;

                        refreshCOMlist();
                        foreach (var item in lbListCOMs.Items)
                        {
                            if (ResolvePortName(item) == lastPort)
                            {
                                lbListCOMs.SelectedItem = item;
                                break;
                            }
                        }

                        ctsReadCOM = new CancellationTokenSource();
                        var task = Task.Run(() => readCom(ctsReadCOM.Token));

                        _ = WaitForDataAsync(3000, lastPort);
                    }));
                    Console.WriteLine("AutoReconnect: connected to " + lastPort);
                }
                catch
                {
                    this.BeginInvoke(new Action(() => refreshCOMlist()));
                }
                finally
                {
                    _autoReconnecting = false;
                }
            });
        }

        /*
         * 
         */
        private void btnComClearLog_Click(object sender, EventArgs e)
        {
            tbComOutput.Text = "";
        }

        /*
         * 
         */
        private void btnComSaveLog_Click(object sender, EventArgs e)
        {
            File.WriteAllText("logs\\COM_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log", tbComOutput.Text);
        }

        /*
         * DQ02 section
         */
        CancellationTokenSource ctsDQ02;
        StreamWriter? dq02LogFile;
        string dq02LogPath = "";
        StreamWriter? dq02HumanLog;
        string dq02HumanLogPath = "";
        DateTime _lastHumanLogWrite = DateTime.MinValue;
        bool _isHumanLogging = false;
        System.Windows.Forms.Timer _blinkTimer;

        private void dq02OpenLog()
        {
            try
            {
                Directory.CreateDirectory("logs");
                dq02LogPath = "logs\\DQ02_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
                dq02LogFile = new StreamWriter(dq02LogPath, false, Encoding.UTF8);
                dq02LogFile.WriteLine("Time;Value;Unit;Secondary;Function;Freq;Level;Circuit;Speed;Range;Nominal;Tolerance;Raw");
                dq02LogFile.Flush();
                Console.WriteLine($"DQ02 log: {dq02LogPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"dq02OpenLog: {ex.Message}");
            }
        }

        private void dq02WriteLog(DQ02Data data)
        {
            try
            {
                if (dq02LogFile != null && data.IsValid)
                {
                    dq02LogFile.WriteLine(
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff};" +
                        $"{data.PrimaryValue.ToString(CultureInfo.InvariantCulture)};" +
                        $"{data.DisplayUnit};" +
                        $"{data.SecondaryValue.ToString(CultureInfo.InvariantCulture)};" +
                        $"{data.Function};" +
                        $"{data.Frequency.ToString(CultureInfo.InvariantCulture)};" +
                        $"{data.Level.ToString(CultureInfo.InvariantCulture)};" +
                        $"{data.CircuitMode};" +
                        $"{data.Speed};" +
                        $"{data.RangeMode};" +
                        $"{data.Nominal.ToString(CultureInfo.InvariantCulture)};" +
                        $"{data.Tolerance.ToString(CultureInfo.InvariantCulture)};" +
                        $"{data.RawLine}"
                    );
                    dq02LogFile.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"dq02WriteLog: {ex.Message}");
            }
        }

        private void dq02CloseLog()
        {
            try
            {
                dq02LogFile?.Close();
                dq02LogFile?.Dispose();
                dq02LogFile = null;
                Console.WriteLine($"DQ02 log closed: {dq02LogPath}");

                _isHumanLogging = false;
                _blinkTimer?.Stop();
                _blinkTimer?.Dispose();
                _blinkTimer = null;
                dq02HumanLog?.Close();
                dq02HumanLog?.Dispose();
                dq02HumanLog = null;
                button8.Text = "ZAPISZ CSV";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"dq02CloseLog: {ex.Message}");
            }
        }

        private void refreshDQ02portlist()
        {
            lbDQ02Ports.DataSource = dq02Comx.listCOMports();
        }

        private async void btnDQ02Connect_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            if (!dq02Comx.isConnected())
            {
                try
                {
                    if (lbDQ02Ports.SelectedItem == null)
                    {
                        MessageBox.Show("No COM port selected. Click REFRESH and select a port.", "DQ02");
                        return;
                    }
                    string port = lbDQ02Ports.SelectedItem.ToString()!;
                    if (!int.TryParse(tbDQ02Baud.Text, out int baud))
                        baud = 115200;

                    dq02Comx.connect(port, baud);
                    dq02OpenLog();

                    btnDQ02Connect.Text = "ROZŁĄCZ " + port;
                    btnDQ02Connect.BackColor = Color.LightCoral;
                    lbDQ02Ports.Enabled = false;
                    tbDQ02Baud.Enabled = false;

                    ctsDQ02 = new CancellationTokenSource();
                    _ = Task.Run(() => readDQ02(ctsDQ02.Token));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection failed:\n{ex.Message}", "DQ02 Error");
                }
            }
            else
            {
                ctsDQ02?.Cancel();
                ctsDQ02?.Dispose();

                dq02Comx.disconnect();
                dq02CloseLog();

                btnDQ02Connect.Text = "POŁĄCZ";
                btnDQ02Connect.BackColor = Color.LightGreen;
                lbDQ02Ports.Enabled = true;
                tbDQ02Baud.Enabled = true;
            }
        }

        async Task readDQ02(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Send FETCh? (LF only, as seen in Wireshark)
                        await dq02Comx.writeAsync("FETCh?\n");

                        // Read one response line
                        string line = "";
                        while (!token.IsCancellationRequested)
                        {
                            int readByte = await dq02Comx.readByteAsync();
                            if (readByte == -1) return;
                            if (readByte == -2) break; // timeout → retry FETCh?

                            byte b = (byte)readByte;

                            if (b == 0x0D || b == 0x0A)
                            {
                                if (line.Length > 0)
                                {
                                    // Got a complete line
                                    break;
                                }
                                // Skip empty bytes between CR/LF
                            }
                            else
                            {
                                line += (char)b;
                            }
                        }

                        if (line.Length > 0)
                        {
                            // Log raw line
                            tbDQ02Log.Invoke(new Action(() =>
                            {
                                tbDQ02Log.AppendText($">> {line}" + Environment.NewLine);
                            }));

                            // Parse and display
                            var parsed = DQ02Data.Parse(line);

                            if (parsed.IsValid)
                                dq02WriteLog(parsed);

                            lblDQ02Value.Invoke(new Action(() =>
                            {
                                if (parsed.IsValid)
                                {
                                    lblDQ02Functions.Text = $"Functions: {parsed.RangeMode}";
                                    button6.Text = $"Tryb: {parsed.RangeMode}";
                                    lblDQ02Speed.Text = $"Speed: {parsed.Speed}";
                                    lblDQ02Model.Text = $"Model: {parsed.ModelMode}";

                                    string freqStr = parsed.Frequency >= 1000
                                        ? (parsed.Frequency / 1000).ToString("F1") + " kHz"
                                        : parsed.Frequency.ToString("F0") + " Hz";
                                    string levelStr = parsed.Level >= 1000
                                        ? (parsed.Level / 1000).ToString("F3") + " V"
                                        : parsed.Level.ToString("F0") + " mV";

                                    lblDQ02Freq.Text = $"Frequency: {freqStr}";
                                    lblDQ02Level.Text = $"Level: {levelStr}";

                                    lblDQ02LossParam.Text = $"Parameters: Loss: {parsed.SecondaryParam}";
                                    lblDQ02Range.Text = $"Range: {DQ02Data.FormatRange(parsed.RangeValue)}";
                                    lblDQ02Output.Text = $"Output: {parsed.Output}";
                                    lblDQ02Comparison.Text = $"Comparison: —";
                                    string biasVoltage = (parsed.MinLimit / 1000.0).ToString("F1") + " V";
                                    lblDQ02Bias.Text = $"Bias: {biasVoltage}";
                                    string userTol = tbDQ02UserTolerance.Text?.Replace("%", "").Trim();
                                    lblDQ02Tolerance.Text = !string.IsNullOrWhiteSpace(userTol)
                                        ? $"Tolerance: {userTol}%"
                                        : parsed.Tolerance > 0
                                            ? $"Tolerance: {parsed.Tolerance:F1}%"
                                            : "Tolerance: —";

                                    lblDQ02Prefix.Text = parsed.DisplayPrefix + ":";
                                    lblDQ02Value.Text = parsed.DisplayValue;
                                    lblDQ02Secondary.Text = parsed.DisplaySecondary;

                                    // User comparison: nominal & tolerance
                                    double userNominal = parsed.Nominal;
                                    if (!string.IsNullOrWhiteSpace(tbDQ02UserNominal.Text))
                                    {
                                        if (!DQ02Data.TryParseUserValue(tbDQ02UserNominal.Text, parsed.Function, out userNominal))
                                            userNominal = -1;
                                    }

                                    string nominalStr = string.IsNullOrWhiteSpace(tbDQ02UserNominal.Text)
                                        ? DQ02Data.FormatNominal(parsed.Nominal, parsed.Function)
                                        : tbDQ02UserNominal.Text.Trim();
                                    lblDQ02Nominal.Text = $"Nominal: {nominalStr}";

                                    double userTolerance = parsed.Tolerance;
                                    if (!string.IsNullOrWhiteSpace(tbDQ02UserTolerance.Text))
                                    {
                                        if (!double.TryParse(tbDQ02UserTolerance.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out userTolerance) &&
                                            !double.TryParse(tbDQ02UserTolerance.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out userTolerance))
                                            userTolerance = -1;
                                    }

                                    lblDQ02Comparison.Text = $"Comparison: {DQ02Data.FormatNominal(userNominal, parsed.Function)} ±{userTolerance:F1}%";

                                    if (userNominal > 0 && !double.IsNaN(parsed.PrimaryValue) && !double.IsInfinity(parsed.PrimaryValue))
                                    {
                                        double deviation = ((parsed.PrimaryValue - userNominal) / userNominal) * 100.0;
                                        lblDQ02Deviation.Text = $"Deviation: {deviation:F2}%";
                                        bool pass = Math.Abs(deviation) <= userTolerance;
                                        lblDQ02PassFail.Text = pass ? "PASS" : "FAIL";
                                        lblDQ02PassFail.ForeColor = pass ? Color.LightGreen : Color.Red;
                                    }
                                    else
                                    {
                                        lblDQ02Deviation.Text = "Deviation: —";
                                        lblDQ02PassFail.Text = "—";
                                    }

                                    // ESR calculation
                                    try
                                    {
                                        double nominalCapacityUf = 0;
                                        string capInput = tbDQ02UserNominal.Text.Trim();
                                        if (!string.IsNullOrWhiteSpace(capInput))
                                        {
                                            bool hasSuffix = capInput.Any(c => !char.IsDigit(c) && c != '.' && c != ',' && c != '-' && c != 'e' && c != 'E');
                                            if (hasSuffix)
                                            {
                                                if (DQ02Data.TryParseUserValue(capInput, parsed.Function, out double capF))
                                                    nominalCapacityUf = capF * 1e6;
                                            }
                                            else
                                            {
                                                if (double.TryParse(capInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) ||
                                                    double.TryParse(capInput, NumberStyles.Any, CultureInfo.CurrentCulture, out val))
                                                    nominalCapacityUf = val;
                                            }
                                        }

                                        double frequency = ParsujCzestotliwosc(lblDQ02Freq.Text);
                                        double capacityF = nominalCapacityUf / 1000000.0;

                                        int voltage = 0;
                                        if (!int.TryParse(textBoxvoltage.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out voltage) &&
                                            !int.TryParse(textBoxvoltage.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out voltage))
                                            voltage = 0;

                                        int celciusTemp = 85;
                                        string tempText = textBoxtemperature.Text.Trim();
                                        if (!string.IsNullOrWhiteSpace(tempText))
                                        {
                                            if (!int.TryParse(tempText, NumberStyles.Any, CultureInfo.InvariantCulture, out celciusTemp) &&
                                                !int.TryParse(tempText, NumberStyles.Any, CultureInfo.CurrentCulture, out celciusTemp))
                                                celciusTemp = 85;
                                        }
                                        double measuredEsr = parsed.SecondaryValue;
                                        if (double.IsNaN(measuredEsr) || measuredEsr <= 0)
                                        {
                                            string esrText = lblDQ02Secondary.Text.Replace(" Ω", "").Replace("ESR:", "").Trim();
                                            if (!double.TryParse(esrText, NumberStyles.Any, CultureInfo.InvariantCulture, out measuredEsr) &&
                                                !double.TryParse(esrText, NumberStyles.Any, CultureInfo.CurrentCulture, out measuredEsr))
                                                measuredEsr = -1;
                                        }

                                        if (nominalCapacityUf > 0 && measuredEsr > 0 && capacityF > 0)
                                        {
                                            double tanDelta = _customTanDelta ?? SzukajTanDeltaZBazy(nominalCapacityUf, voltage, celciusTemp);
                                            label6.Text = _customTanDelta.HasValue ? $"tanδ = {tanDelta:F4} (RĘCZNIE)" : $"tanδ = {tanDelta:F4}";
                                            double targetEsr = Math.Round(tanDelta / (2 * Math.PI * frequency * capacityF), 3);
                                            double targetEsrMiliOmy = Math.Round(targetEsr * 1000, 0);

                                            lblTargetESR.Text = $"{targetEsr} Ω ({targetEsrMiliOmy} mΩ) | mierzony: {measuredEsr * 1000:F0} mΩ";

                                            double limitWarning = targetEsr * 1.5;
                                            double limitReplace = targetEsr * 2.0;

                                            if (measuredEsr <= limitWarning)
                                            {
                                                lblStatus.Text = "Kondensator jest SPRAWNY";
                                                lblStatus.ForeColor = Color.Green;
                                            }
                                            else if (measuredEsr <= limitReplace)
                                            {
                                                lblStatus.Text = "Kondensator już LEKKO WYSYCHA";
                                                lblStatus.ForeColor = Color.Orange;
                                            }
                                            else
                                            {
                                                lblStatus.Text = "Kondensator jest USZKODZONY";
                                                lblStatus.ForeColor = Color.Red;
                                            }
                                        }
                                        else
                                        {
                                            label6.Text = "tanδ: —";
                                            lblTargetESR.Text = "ESR: —";
                                            lblStatus.Text = "STATUS: —";
                                            lblStatus.ForeColor = Color.FromArgb(200, 200, 200);
                                        }
                                    }
                                    catch
                                    {
                                        label6.Text = "tanδ: —";
                                        lblTargetESR.Text = "ESR: —";
                                        lblStatus.Text = "BŁĄD DANYCH!";
                                        lblStatus.ForeColor = Color.DarkRed;
                                    }

                                    standardDisplayPanel.SetDQ02Value(parsed.DisplayPrefix, parsed.DisplayValue, parsed.DisplaySecondary);

                                    // Human log while button8 logging active
                                    if (_isHumanLogging && dq02HumanLog != null && (DateTime.Now - _lastHumanLogWrite).TotalMilliseconds >= 500)
                                    {
                                        _lastHumanLogWrite = DateTime.Now;
                                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        dq02HumanLog.WriteLine($"{now};{tbDQ02UserNominal.Text.Trim()};{textBoxvoltage.Text.Trim()};{textBoxtemperature.Text.Trim()};{lblDQ02Value.Text.Trim()};{lblDQ02Secondary.Text.Trim()};{lblTargetESR.Text.Trim()};{lblStatus.Text.Trim()};{lblDQ02Comparison.Text.Trim()};{lblDQ02PassFail.Text.Trim()};{lblDQ02Deviation.Text.Trim()};{lblDQ02Freq.Text.Trim()};{lblDQ02Nominal.Text.Trim()}");
                                        dq02HumanLog.Flush();
                                    }
                                }
                            }));
                        }

                        // Small delay before next poll
                        await Task.Delay(50);
                    }
                    catch (TimeoutException)
                    {
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"readDQ02: {ex.Message}");
                        await Task.Delay(200);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("readDQ02 OperationCanceledException");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"readDQ02 {ex.Message}");
            }

            // Update UI on disconnect
            try
            {
                btnDQ02Connect.Invoke(new Action(() =>
                {
                    btnDQ02Connect.Text = "POŁĄCZ";
                    btnDQ02Connect.BackColor = Color.LightGreen;
                    lbDQ02Ports.Enabled = true;
                    tbDQ02Baud.Enabled = true;
                }));
            }
            catch { }

            Console.WriteLine("Task readDQ02 finish");
        }

        private void btnDQ02RefreshPorts_Click(object sender, EventArgs e)
        {
            refreshDQ02portlist();
        }

        private void btnDQ02ClearLog_Click(object sender, EventArgs e)
        {
            tbDQ02Log.Text = "";
        }

        private void cbDQ02LogToggle_CheckedChanged(object sender, EventArgs e)
        {
            tbDQ02Log.Visible = !checkBox1.Checked;
        }

        private void btnDQ02SaveLog_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("logs");
            File.WriteAllText("logs\\DQ02_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log", tbDQ02Log.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            if (dq02Comx.isConnected())
            {
                try
                {
                    dq02Comx.write("FREQuency\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Port COM jest zamknięty!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            if (dq02Comx.isConnected())
            {
                try
                {
                   // dq02Comx.write("VOLTage\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Port COM jest zamknięty!");
            }

            string cap = tbDQ02UserNominal.Text.Trim();
            string tol = tbDQ02UserTolerance.Text.Trim().Replace("%", "");
            string volt = textBoxvoltage.Text.Trim();
            string temp = textBoxtemperature.Text.Trim();
            string tdelta = label6.Text.Trim();
            string esr = lblDQ02Secondary.Text.Trim();
            string targetEsr = lblTargetESR.Text.Trim();
            string status = lblStatus.Text.Trim();

            string query = "Powiedz mi wszystko o mierzonym kondensatorze: "
                + $"pojemność {cap}, tolerancja {tol}%, napięcie {volt}V, "
                + $"temperatura {temp}°C, tanδ {tdelta}, "
                + $"zmierzony ESR {esr}, ESR docelowy {targetEsr}, "
                + $"status: {status}"
                + $" Znajdź kondensatory z tymi parametrami na AliExpress, Allegro, Mouser, TME i innych sklepach.";

            try
            {
                string url = "https://www.google.com/search?q=" + Uri.EscapeDataString(query);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie można otworzyć przeglądarki: {ex.Message}");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            try
            {
                if (!_isHumanLogging)
                {
                    // Start logging
                    Directory.CreateDirectory("logs");
                    dq02HumanLogPath = "logs\\DQ02_human_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
                    dq02HumanLog = new StreamWriter(dq02HumanLogPath, false, Encoding.UTF8);
                    dq02HumanLog.WriteLine("Czas;Pojemnosc_nominalna_uF;Napiecie_V;Temperatura_C;Wartosc_glowna;ESR_zmierzony;ESR_docelowy;Status;Porownanie;PassFail;Odchylenie;Czestotliwosc;Wartosc_nominalna");
                    dq02HumanLog.Flush();
                    _isHumanLogging = true;
                    _lastHumanLogWrite = DateTime.MinValue;

                    _blinkTimer = new System.Windows.Forms.Timer { Interval = 500 };
                    bool blinkState = false;
                    _blinkTimer.Tick += (_, _) =>
                    {
                        blinkState = !blinkState;
                        button8.Text = blinkState ? "" : "ZAPISUJE...";
                    };
                    _blinkTimer.Start();
                    button8.Text = "ZAPISUJE...";
                }
                else
                {
                    // Stop logging
                    _isHumanLogging = false;
                    _blinkTimer?.Stop();
                    _blinkTimer?.Dispose();
                    _blinkTimer = null;

                    dq02HumanLog?.Flush();
                    dq02HumanLog?.Close();
                    dq02HumanLog?.Dispose();
                    dq02HumanLog = null;

                    using var sfd = new SaveFileDialog
                    {
                        Filter = "CSV (*.csv)|*.csv",
                        FileName = Path.GetFileName(dq02HumanLogPath),
                        InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(dq02HumanLogPath))
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.Copy(dq02HumanLogPath, sfd.FileName, true);
                        MessageBox.Show($"Zapisano: {sfd.FileName}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    button8.Text = "ZAPISZ CSV";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private double ParsujCzestotliwosc(string freqText)
        {
            string clean = freqText.ToLowerInvariant().Replace("frequency:", "").Replace(" ", "").Trim();
            if (clean.Contains("khz"))
            {
                string digits = clean.Replace("khz", "");
                if (double.TryParse(digits, NumberStyles.Any, CultureInfo.InvariantCulture, out double f) ||
                    double.TryParse(digits, NumberStyles.Any, CultureInfo.CurrentCulture, out f))
                    return f * 1000;
            }
            if (clean.Contains("hz"))
            {
                string digits = clean.Replace("hz", "");
                if (double.TryParse(digits, NumberStyles.Any, CultureInfo.InvariantCulture, out double f) ||
                    double.TryParse(digits, NumberStyles.Any, CultureInfo.CurrentCulture, out f))
                    return f;
            }
            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) ||
                double.TryParse(clean, NumberStyles.Any, CultureInfo.CurrentCulture, out val))
                return val;
            return 120;
        }

        private void ZaladujBazeEsr()
        {
            if (_bazaEsr != null) return;
            lock (_bazaLock)
            {
                if (_bazaEsr != null) return;
                string path = Path.Combine(Application.StartupPath, "baza_esr.csv");
                if (!File.Exists(path)) { _bazaEsr = new List<EsrCsvRow>(0); return; }

                _bazaEsr = File.ReadAllLines(path)
                    .Skip(1)
                    .Select(line => line.Split(','))
                    .Where(parts => parts.Length == 4)
                    .Select(parts => new EsrCsvRow
                    {
                        PojemnoscUf = double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0,
                        NapiecieV = double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0,
                        TemperaturaC = double.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0,
                        MaxTanDelta = double.TryParse(parts[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0,
                    })
                    .ToList();
            }
        }

        private double SzukajTanDeltaZBazy(double capacityUf, int voltage, int temperature)
        {
            ZaladujBazeEsr();
            if (_bazaEsr == null || _bazaEsr.Count == 0)
                return ObliczTanDeltaSzczegolowo(voltage, temperature >= 95);

            double tempTarget = temperature >= 95 ? 105 : 85;

            var candidates = _bazaEsr
                .Where(r => Math.Abs(r.TemperaturaC - tempTarget) < 0.5)
                .OrderBy(r => Math.Abs(r.NapiecieV - voltage))
                .ThenBy(r => Math.Abs(r.PojemnoscUf - capacityUf))
                .ToList();

            if (candidates.Count == 0)
                return ObliczTanDeltaSzczegolowo(voltage, temperature >= 95);

            var best = candidates.First();
            double capDiff = Math.Abs(best.PojemnoscUf - capacityUf);
            double voltDiff = Math.Abs(best.NapiecieV - voltage);

            // If closest match is far off, fall back to formula
            if (voltDiff > voltage * 0.5 + 10 && capDiff > capacityUf * 0.5 + 10)
                return ObliczTanDeltaSzczegolowo(voltage, temperature >= 95);

            return best.MaxTanDelta;
        }

        private double ObliczTanDeltaSzczegolowo(int vol, bool is105)
        {
            if (vol <= 2)   return is105 ? 0.24 : 0.26;
            if (vol <= 4)   return is105 ? 0.22 : 0.24;
            if (vol <= 6)   return is105 ? 0.20 : 0.22;
            if (vol <= 7)   return is105 ? 0.18 : 0.20;
            if (vol <= 10)  return is105 ? 0.16 : 0.19;
            if (vol <= 16)  return is105 ? 0.14 : 0.16;
            if (vol <= 20)  return is105 ? 0.13 : 0.15;
            if (vol <= 25)  return is105 ? 0.12 : 0.14;
            if (vol <= 35)  return is105 ? 0.10 : 0.12;
            if (vol <= 50)  return is105 ? 0.08 : 0.10;
            if (vol <= 63)  return is105 ? 0.08 : 0.09;
            if (vol <= 80)  return is105 ? 0.08 : 0.08;
            if (vol <= 100) return is105 ? 0.07 : 0.08;
            if (vol <= 125) return is105 ? 0.08 : 0.10;
            if (vol <= 160) return is105 ? 0.12 : 0.15;
            if (vol <= 200) return is105 ? 0.12 : 0.15;
            if (vol <= 250) return is105 ? 0.12 : 0.15;
            if (vol <= 315) return is105 ? 0.15 : 0.15;
            if (vol <= 350) return is105 ? 0.15 : 0.20;
            if (vol <= 385) return is105 ? 0.15 : 0.20;
            if (vol <= 400) return is105 ? 0.15 : 0.20;
            if (vol <= 420) return is105 ? 0.17 : 0.20;
            if (vol <= 450) return is105 ? 0.20 : 0.20;
            if (vol <= 500) return is105 ? 0.20 : 0.25;
            if (vol <= 550) return is105 ? 0.22 : 0.25;
            if (vol <= 600) return is105 ? 0.24 : 0.30;
            if (vol <= 650) return is105 ? 0.24 : 0.30;
            if (vol <= 710) return is105 ? 0.25 : 0.32;
            return is105 ? 0.25 : 0.35;
        }
    }
}
