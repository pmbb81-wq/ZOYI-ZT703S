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

                                    string nominalStr = DQ02Data.FormatNominal(parsed.Nominal, parsed.Function);
                                    lblDQ02Nominal.Text = $"Nominal: {nominalStr}";

                                    lblDQ02LossParam.Text = $"Parameters: Loss: {parsed.SecondaryParam}";
                                    lblDQ02Range.Text = $"Range: {DQ02Data.FormatRange(parsed.RangeValue)}";
                                    lblDQ02Output.Text = $"Output: {parsed.Output}";
                                    lblDQ02Comparison.Text = $"Comparison: —";
                                    string biasVoltage = (parsed.MinLimit / 1000.0).ToString("F1") + " V";
                                    lblDQ02Bias.Text = $"Bias: {biasVoltage}";
                                    lblDQ02Tolerance.Text = parsed.Tolerance > 0
                                        ? $"Tolerance: {parsed.Tolerance:F1}%"
                                        : "Tolerance: —";

                                    lblDQ02Prefix.Text = parsed.DisplayPrefix + ":";
                                    lblDQ02Value.Text = parsed.DisplayValue;
                                    lblDQ02Secondary.Text = parsed.DisplaySecondary;

                                    // User comparison: nominal & tolerance
                                    double userNominal = parsed.Nominal;
                                    if (double.TryParse(tbDQ02UserNominal.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double un) && un > 0)
                                        userNominal = un;

                                    double userTolerance = parsed.Tolerance;
                                    if (double.TryParse(tbDQ02UserTolerance.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double ut) && ut > 0)
                                        userTolerance = ut;

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
    }
}
