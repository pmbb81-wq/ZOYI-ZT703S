using System.IO.Ports;

namespace ZOYI;

public partial class MainWindow
{
    private Riden6012 _riden = new();
    private const int MaxLogLines = 200;
    private System.Windows.Forms.Timer? _blinkUsb;
    private bool _blinkState;
    private System.Windows.Forms.Timer? _blinkOutput;
    private bool _blinkOutputState;

    private void InitializeRidenTab()
    {
        _riden.OnDataUpdated += RidenDataUpdated;
        _riden.OnDebugData += RidenDebugData;

        ridenBtnConnect.Click += RidenBtnConnect_Click;
        ridenBtnOn.Click += (_, _) => { if (_riden.Polaczony) { _riden.ZalaczWyjscie(); Log("ON"); } };
        ridenBtnOff.Click += (_, _) => { if (_riden.Polaczony) { _riden.WylaczWyjscie(); Log("OFF"); } };
        ridenBtnSetV.Click += (_, _) => RidenSetV();
        ridenBtnSetI.Click += (_, _) => RidenSetI();
        ridenBtnSetOCP.Click += ridenBtnSetOCP_Click;
        ridenBtnClearLog.Click += (_, _) => ridenTbLog.Clear();

        ridenBtnConnectUSB.Click += RidenBtnConnectUSB_Click;
        ridenBtnRefreshPorts.Click += (_, _) => OdswiezPortyCOM();
        OdswiezPortyCOM();

        _blinkUsb = new System.Windows.Forms.Timer { Interval = 500 };
        _blinkUsb.Tick += (_, _) =>
        {
            _blinkState = !_blinkState;
            ridenBtnConnectUSB.BackColor = _blinkState ? Color.DarkGreen : Color.FromArgb(0, 60, 0);
        };

        _blinkOutput = new System.Windows.Forms.Timer { Interval = 500 };
        _blinkOutput.Tick += (_, _) =>
        {
            _blinkOutputState = !_blinkOutputState;
            ridenLblOutputVal.ForeColor = _blinkOutputState ? Color.LimeGreen : Color.FromArgb(34, 34, 34);
        };

        ridenTxtVset.Text = Properties.Settings.Default.riden_vset;
        ridenTxtIset.Text = Properties.Settings.Default.riden_iset;
        ridenTxtOVP.Text = Properties.Settings.Default.riden_ovp;
        if (float.TryParse(Properties.Settings.Default.riden_ovp, out float ovpStart))
            ridenLblOvpVal.Text = ovpStart.ToString("F2") + " V";
    }

    private void OdswiezPortyCOM()
    {
        ridenCmbPort.Items.Clear();
        foreach (string port in SerialPort.GetPortNames())
            ridenCmbPort.Items.Add(port);
        if (ridenCmbPort.Items.Count > 0)
            ridenCmbPort.SelectedIndex = 0;
    }

    private void RidenDataUpdated()
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            try { Invoke((Action)RidenDataUpdated); } catch (ObjectDisposedException) { }
            return;
        }

        bool connected = _riden.Polaczony;
        labeltemp.Text         = "TEMP: " + _riden.Temperature + " °C";
        ridenLblVsetVal.Text   = _riden.Vset.ToString("F2") + " V";
        ridenLblIsetVal.Text   = _riden.Iset.ToString("F3") + " A";
        ridenLblVoutVal.Text   = _riden.Vout.ToString("F2") + " V";
        label7.Text            = $"VIN: {_riden.Vin.ToString("F2")} V   ({_riden.RawVin} / 0x{_riden.RawVin:X4})";
        ridenLblIoutVal.Text   = _riden.Iout.ToString("F2") + " A";
        ridenLblPowerVal.Text  = _riden.Power.ToString("F2") + " W";
        ridenLblOutputVal.Text = _riden.OutputOn ? "ON" : "OFF";
        if (_riden.OutputOn)
        {
            if (!_blinkOutput!.Enabled)
            {
                _blinkOutputState = true;
                ridenLblOutputVal.ForeColor = Color.LimeGreen;
                _blinkOutput.Start();
            }
        }
        else
        {
            _blinkOutput!.Stop();
            ridenLblOutputVal.ForeColor = Color.Gray;
        }

        if (_riden.OVP > 0)
            ridenLblOvpVal.Text = _riden.OVP.ToString("F2") + " V";
        if (_riden.OCP > 0)
            ridenLblOcpVal.Text = _riden.OCP.ToString("F2") + " A";

        ridenLblStatus.Text = $"Polaczony | OUT: {(_riden.OutputOn ? "ON" : "OFF")}";

        ridenBtnSetV.Enabled = connected;
        ridenBtnSetI.Enabled = connected;
        ridenBtnSetOVP.Enabled = connected;
        ridenBtnSetOCP.Enabled = connected;
        ridenBtnOn.Enabled = connected;
        ridenBtnOff.Enabled = connected;

        if (_riden.TrybUSB && connected)
        {
            ridenLblPortStatus.Text = "USB: polaczony";
            ridenLblPortStatus.ForeColor = Color.LimeGreen;
        }
        else if (!_riden.TrybUSB)
        {
            ridenLblPortStatus.Text = "USB: ---";
            ridenLblPortStatus.ForeColor = Color.Gray;
        }
    }

    private void RidenDebugData(string msg)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            try { Invoke(() => RidenDebugData(msg)); } catch (ObjectDisposedException) { }
            return;
        }
        ridenTbLog.AppendText(msg + "\r\n");
        var lines = ridenTbLog.Lines;
        if (lines.Length > MaxLogLines)
        {
            ridenTbLog.Lines = lines[(lines.Length - MaxLogLines)..];
        }
        ridenTbLog.SelectionStart = ridenTbLog.TextLength;
        ridenTbLog.ScrollToCaret();
    }

    private void RidenBtnConnect_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_riden.Polaczony)
            {
                _riden.Rozlacz();
                _blinkUsb?.Stop();
                Log("Serwer zatrzymany.");
                ridenBtnConnect.Text = "START SERWER";
                ridenBtnConnect.BackColor = Color.LightGreen;
                ridenLblStatus.Text = "Serwer zatrzymany.";
                ridenLblStatus.ForeColor = Color.Gray;
                ridenCmbPort.Enabled = true;
                ridenBtnRefreshPorts.Enabled = true;
                ridenBtnConnectUSB.Text = "POLACZ USB";
                ridenBtnConnectUSB.BackColor = Color.FromArgb(0, 100, 0);
                return;
            }

            Log("Uruchamianie serwera na porcie 8080...");
            AktualizujLocalIP();
            _riden.Polacz();
            ridenLblStatus.Text = "Serwer nasluchuje na porcie 8080...";
            ridenLblStatus.ForeColor = Color.Yellow;
            ridenBtnConnect.Text = "ZATRZYMAJ SERWER";
            ridenBtnConnect.BackColor = Color.Orange;
            ridenBtnSetV.Enabled = false;
            ridenBtnSetI.Enabled = false;
            ridenBtnOn.Enabled = false;
            ridenBtnOff.Enabled = false;
        }
        catch (Exception ex)
        {
            string msg = $"{ex.GetType().Name}: {ex.Message}";
            ridenLblStatus.Text = msg;
            ridenLblStatus.ForeColor = Color.Red;
            Log("BLAD: " + msg);
        }
    }

    private static float ParseValue(string input, float min, float max, string unit)
    {
        input = input.Trim().Replace(',', '.');
        float mul = 1;
        if (input.EndsWith("mA", StringComparison.OrdinalIgnoreCase)) { mul = 0.001f; input = input[..^2]; }
        else if (input.EndsWith("mV", StringComparison.OrdinalIgnoreCase)) { mul = 0.001f; input = input[..^2]; }
        else if (input.EndsWith("A", StringComparison.OrdinalIgnoreCase) || input.EndsWith("V", StringComparison.OrdinalIgnoreCase)) input = input[..^1];

        if (float.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float val))
            return Math.Clamp(val * mul, min, max);
        return float.NaN;
    }

    private async void RidenBtnConnectUSB_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_riden.Polaczony)
            {
                _riden.Rozlacz();
                _blinkUsb?.Stop();
                Log("USB rozlaczony.");
                ridenBtnConnectUSB.Text = "POLACZ USB";
                ridenBtnConnectUSB.BackColor = Color.FromArgb(0, 100, 0);
                ridenLblPortStatus.Text = "USB: rozlaczony";
                ridenLblPortStatus.ForeColor = Color.Gray;
                ridenCmbPort.Enabled = true;
                ridenBtnRefreshPorts.Enabled = true;
                return;
            }

            if (ridenCmbPort.SelectedItem is not string port)
            {
                Log("Wybierz port COM.");
                return;
            }

            Log($"Laczenie przez {port}...");
            _riden.PolaczSerial(port);
            ridenLblPortStatus.Text = $"USB: {port} - l aczenie...";
            ridenLblPortStatus.ForeColor = Color.Yellow;
            ridenBtnConnectUSB.Text = "ROZŁACZ USB";
            _blinkState = false;
            _blinkUsb?.Start();
            ridenCmbPort.Enabled = false;
            ridenBtnRefreshPorts.Enabled = false;
            ridenBtnConnect.Text = "START SERWER";
            ridenBtnConnect.BackColor = Color.LightGreen;
            ridenLblStatus.Text = "USB aktywne";
            ridenLblStatus.ForeColor = Color.Yellow;
            ridenLblLocalIP.Text = "IP: ---";
            ridenBtnSetV.Enabled = false;
            ridenBtnSetI.Enabled = false;
            ridenBtnOn.Enabled = false;
            ridenBtnOff.Enabled = false;

            await System.Threading.Tasks.Task.Delay(500);
            RidenSetV();
            RidenSetI();
        }
        catch (Exception ex)
        {
            string msg = $"{ex.GetType().Name}: {ex.Message}";
            ridenLblPortStatus.Text = msg;
            ridenLblPortStatus.ForeColor = Color.Red;
            Log("BLAD USB: " + msg);
        }
    }

    private void RidenSetV()
    {
        if (!_riden.Polaczony) { Log("Nie polaczony z zasilaczem."); return; }
        float v = ParseValue(ridenTxtVset.Text, 0, 60, "V");
        if (float.IsNaN(v)) { Log("Blad parsowania: " + ridenTxtVset.Text); return; }
        _riden.UstawNapiecie(v);
        Log($"Ustawiam napiecie: {v:F2} V");
    }

    private void RidenSetI()
    {
        if (!_riden.Polaczony) { Log("Nie polaczony z zasilaczem."); return; }
        float i = ParseValue(ridenTxtIset.Text, 0, 20, "A");
        if (float.IsNaN(i)) { Log("Blad parsowania: " + ridenTxtIset.Text); return; }
        _riden.UstawPrad(i);
        Log($"Ustawiam prad: {i:F3} A");
    }

    private void ridenBtnSetOCP_Click(object sender, EventArgs e)
    {
        if (!_riden.Polaczony) { Log("Nie polaczony z zasilaczem."); return; }
        float f = ParseValue(ridenTxtOCP.Text, 0, 20, "A");
        if (float.IsNaN(f)) { Log("Blad parsowania: " + ridenTxtOCP.Text); return; }
        _riden.UstawOCP(f);
        Log($"OCP ustawione na {f:F2} A");
    }

    private void Log(string msg)
    {
        RidenDebugData($"[{DateTime.Now:HH:mm:ss}] {msg}");
    }

    private void AktualizujLocalIP()
    {
        var ips = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
            .AddressList
            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .Select(ip => ip.ToString());
        ridenLblLocalIP.Text = "IP: " + string.Join(", ", ips);
    }
}
