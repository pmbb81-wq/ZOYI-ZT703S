using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
                    COMportName = lbListCOMs.SelectedItem!.ToString();
                    int baudrate = int.Parse(tbCOMBaudrate.Text);

                    comx.connect(COMportName!, baudrate);

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
                    //MessageBox.Show("btnComConnect_Click:" + ex);
                    Console.WriteLine(ex);
                }
            }
            else
            {
                ctsReadCOM.Cancel();
                ctsReadCOM.Dispose();

                comx.disconnect();

                btnComConnect.Text = "POŁĄCZ";
                btnComConnect.BackColor = Color.LightGreen;
                lblComConnStatus.Text = "ROZŁĄCZONY";
                lblComConnStatus.ForeColor = Color.LightGreen;

                lbListCOMs.Enabled = true;
                tbCOMBaudrate.Enabled = true;
            }
        }

        async Task readCom(CancellationToken token)
        {
            String buff = "";
            byte[] bytesArray = new byte[18];
            int indexBytesArray = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        int readByte = await comx.readByteAsync();

                        if (readByte == -1)
                            continue;

                        char c = (char)readByte;

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
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("readCom panel update Exception");
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
        void refreshCOMlist()
        {
            lbListCOMs.DataSource = comx.listCOMports();
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
            if (dq02Comx.isConnected())
            {
                try
                {
                    dq02Comx.write("VOLTage\n");
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
