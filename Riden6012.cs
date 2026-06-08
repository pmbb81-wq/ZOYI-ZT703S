using System.Net.Sockets;
using System.Net;
using System.IO.Ports;
using System.Text;
using System.Collections.Concurrent;

namespace ZOYI;

public class Riden6012 : IDisposable
{
    private TcpListener? _listener;
    private TcpClient? _client;
    private SerialPort? _serialPort;
    private Stream? _stream;
    private Thread? _worker;
    private CancellationTokenSource? _cts;
    private readonly ushort[] _registers = new ushort[256];
    private readonly object _lock = new();
    private readonly ConcurrentQueue<(byte adres, ushort wartosc)> _pendingWrites = new();

    public bool Polaczony => _client?.Connected ?? _serialPort?.IsOpen ?? false;
    public bool SerwerActive => _listener is not null;
    public string OstatniBlad { get; private set; } = "";
    public bool TrybUSB => _serialPort?.IsOpen ?? false;

    public ushort RawVout => _registers[10];
    public float Vout => _registers[10] / 100f;
    public float Iout => _registers[11] / 100f;
    public float Power => _registers[13] / 100f;
    public float Vset => _registers[8] / 100f;
    public float Iset => _registers[9] / 100f;
    public ushort RawVin => _registers[14];
    public float Vin => _registers[14] / 100f;
    public float OVP => _registers[82] / 100f;
    public float OCP => _registers[83] / 100f;
    public int Temperature => _registers[4] == 0 ? _registers[5] : -_registers[5];
    public bool OutputOn => _registers[18] == 1;

    public event Action? OnDataUpdated;
    public event Action<string>? OnDebugData;

    public void Polacz(int port = 8080)
    {
        Rozlacz();
        OstatniBlad = "";

        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        OnDebugData?.Invoke($"Serwer nasluchuje na porcie {port}...");

        _cts = new CancellationTokenSource();
        _worker = new Thread(() => WorkerLoopTcp(_cts.Token));
        _worker.IsBackground = true;
        _worker.Start();
    }

    public void PolaczSerial(string portName, int baudRate = 115200)
    {
        Rozlacz();
        OstatniBlad = "";

        _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        _serialPort.ReadTimeout = 500;
        _serialPort.Open();
        _stream = _serialPort.BaseStream;

        OnDebugData?.Invoke($"COM {portName} otwarty ({baudRate} baud)");

        byte[] query = Encoding.ASCII.GetBytes("queryd\r\n");
        _stream.Write(query, 0, query.Length);
        OnDebugData?.Invoke(">> queryd");

        _cts = new CancellationTokenSource();
        _worker = new Thread(() => WorkerLoopSerial(_cts.Token));
        _worker.IsBackground = true;
        _worker.Start();
    }

    private void WorkerLoopTcp(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                try { OnDebugData?.Invoke("Oczekiwanie na polaczenie od zasilacza..."); } catch (ObjectDisposedException) { }

                var acceptTask = _listener!.AcceptTcpClientAsync();
                acceptTask.Wait(ct);
                _client = acceptTask.Result;
                _stream = _client.GetStream();
                _stream.ReadTimeout = 1000;

                try { OnDebugData?.Invoke($"Polaczono z {_client.Client.RemoteEndPoint}!"); } catch (ObjectDisposedException) { }

                SynchronizujStanPoczatkowy();

                PetlaOdczytu(ct, () => _client?.Connected ?? false);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (Exception ex)
            {
                try { OnDebugData?.Invoke("BLAD: " + ex.Message); } catch (ObjectDisposedException) { }
                Thread.Sleep(1000);
            }
            finally
            {
                if (!ct.IsCancellationRequested)
                {
                    try { _stream?.Close(); } catch { }
                    try { _client?.Close(); } catch { }
                    _stream = null;
                    _client = null;
                    try { OnDataUpdated?.Invoke(); } catch (ObjectDisposedException) { }
                }
            }
        }
    }

    private void WorkerLoopSerial(CancellationToken ct)
    {
        try { OnDataUpdated?.Invoke(); } catch (ObjectDisposedException) { }
        PetlaOdczytu(ct, () => _serialPort?.IsOpen ?? false);

        try { _stream?.Close(); } catch { }
        try { _serialPort?.Close(); } catch { }
        _stream = null;
        _serialPort = null;
        try { OnDataUpdated?.Invoke(); } catch (ObjectDisposedException) { }
    }

    private void SynchronizujStanPoczatkowy()
    {
        try
        {
            byte[] req = new byte[8];
            req[0] = 0x01;
            req[1] = 0x03;
            req[2] = 0x00;
            req[3] = 0x00;
            req[4] = 0x00;
            req[5] = 0x78;
            byte[] crc = CalculateCRC(req, 0, 6);
            req[6] = crc[0];
            req[7] = crc[1];
            var s = _stream;
            if (s is null) return;
            lock (_lock) { s.Write(req, 0, req.Length); }
            OnDebugData?.Invoke(">> " + BitConverter.ToString(req) + " (sync)");

            byte[] buf = new byte[512];
            int pos = 0;
            int attempts = 0;
            while (attempts < 20)
            {
                int read = s.Read(buf, pos, buf.Length - pos);
                if (read == 0) return;
                pos += read;
                OnDebugData?.Invoke("<< " + BitConverter.ToString(buf, 0, pos) + " (sync)");

                int consumed = ParseFrame(buf, 0, pos);
                if (consumed > 0)
                {
                    OnDebugData?.Invoke("Synchronizacja stanu OK");
                    return;
                }
                attempts++;
            }
            OnDebugData?.Invoke("Synchronizacja: niekompletna ramka");
        }
        catch (Exception ex)
        {
            OnDebugData?.Invoke("BLAD synchronizacji: " + ex.Message);
        }
    }

    private void PetlaOdczytu(CancellationToken ct, Func<bool> connected)
    {
        byte[] buffer = new byte[1024];
        int pos = 0;

        while (!ct.IsCancellationRequested && connected())
        {
            try
            {
                var stream = _stream;
                if (stream is null) { Thread.Sleep(100); continue; }
                OproczKolejkeZapisow();
                byte[] req = MakeReadRequest();
                lock (_lock) { stream.Write(req, 0, req.Length); }
                try { OnDebugData?.Invoke(">> " + BitConverter.ToString(req)); } catch { }

                for (;;)
                {
                    if (!connected() || ct.IsCancellationRequested) return;
                    if (pos == 0)
                    {
                        int read = stream.Read(buffer, 0, buffer.Length);
                        if (read == 0) return;
                        pos = read;
                        try { OnDebugData?.Invoke("<< " + BitConverter.ToString(buffer, 0, read)); } catch { }
                    }

                    int consumed = ParseFrame(buffer, 0, pos);
                    if (consumed == 0)
                    {
                        if (!connected() || ct.IsCancellationRequested) return;
                        int read = stream.Read(buffer, pos, buffer.Length - pos);
                        if (read == 0) return;
                        pos += read;
                        try { OnDebugData?.Invoke("<< " + BitConverter.ToString(buffer, pos - read, read)); } catch { }
                        continue;
                    }
                    if (consumed == 1)
                    {
                        Array.Copy(buffer, 1, buffer, 0, pos - 1);
                        pos--;
                        break;
                    }
                    if (consumed < pos)
                        Array.Copy(buffer, consumed, buffer, 0, pos - consumed);
                    pos -= consumed;
                    if (pos == 0) break;
                }
            }
            catch (IOException) when (!ct.IsCancellationRequested) { }
            catch (TimeoutException) when (!ct.IsCancellationRequested) {
                try { OnDebugData?.Invoke("<< [timeout]"); } catch { }
            }
            catch (OperationCanceledException) { if (ct.IsCancellationRequested) break; }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                try { OnDebugData?.Invoke("BLAD: " + ex.GetType().Name + ": " + ex.Message); } catch { }
            }
        }
    }

    private byte[] MakeReadRequest()
    {
        byte[] req = new byte[8];
        req[0] = 0x01;
        req[1] = 0x03;
        req[2] = 0x00;
        req[3] = 0x00;
        req[4] = 0x00;
        req[5] = 0x78;
        byte[] crc = CalculateCRC(req, 0, 6);
        req[6] = crc[0];
        req[7] = crc[1];
        return req;
    }

    private int ParseFrame(byte[] buf, int off, int len)
    {
        if (len < 4) return 0;
        byte slave = buf[off];
        byte func = buf[off + 1];

        if ((func & 0x80) != 0)
        {
            byte excCode = (byte)(func & 0x7F);
            if (len < 5) return 0;
            byte[] crcRsp = CalculateCRC(buf, off, 3);
            if (crcRsp[0] != buf[off + 3] || crcRsp[1] != buf[off + 4])
                return 1;
            OnDebugData?.Invoke($"<< BLAD Modbus: func=0x{excCode:X2}, code={buf[off + 2]}");
            return 5;
        }

        if (func == 0x03)
        {
            if (len < 8) return 0;

            byte byteCount = buf[off + 2];
            int dataLen = 3 + byteCount + 2;
            if (len < dataLen) return 0;

            byte[] crcRsp = CalculateCRC(buf, off, dataLen - 2);
            if (crcRsp[0] != buf[off + dataLen - 2] || crcRsp[1] != buf[off + dataLen - 1])
                return 1;

            for (int i = 0; i < byteCount / 2; i++)
            {
                if (i < _registers.Length)
                    _registers[i] = (ushort)((buf[off + 3 + i * 2] << 8) | buf[off + 3 + i * 2 + 1]);
            }

            OnDataUpdated?.Invoke();
            return dataLen;
        }

        if (func == 0x06 && slave == 0x01)
        {
            if (len < 8) return 0;

            byte[] crcRsp = CalculateCRC(buf, off, 6);
            if (crcRsp[0] != buf[off + 6] || crcRsp[1] != buf[off + 7])
                return 1;

            ushort addr = (ushort)((buf[off + 2] << 8) | buf[off + 3]);
            ushort val = (ushort)((buf[off + 4] << 8) | buf[off + 5]);
            if (addr < _registers.Length)
                _registers[addr] = val;
            OnDataUpdated?.Invoke();

            return 8;
        }

        if (func == 0x10 && slave == 0x01)
        {
            if (len < 8) return 0;

            byte[] crcRsp = CalculateCRC(buf, off, 6);
            if (crcRsp[0] != buf[off + 6] || crcRsp[1] != buf[off + 7])
                return 1;

            return 8;
        }

        return 1;
    }

    public void Rozlacz()
    {
        _cts?.Cancel();
        try { _stream?.Close(); } catch { }
        try { _client?.Close(); } catch { }
        try { _listener?.Stop(); } catch { }
        try { _serialPort?.Close(); } catch { }
        _stream = null;
        _client = null;
        _listener = null;
        _serialPort = null;
    }

    public void ZalaczWyjscie() => WyslijZapis(18, 1);
    public void WylaczWyjscie() => WyslijZapis(18, 0);
    public void UstawNapiecie(float volts) => WyslijZapis(8, (ushort)(volts * 100));
    public void UstawPrad(float amps) => WyslijZapis(9, (ushort)(amps * 100));
    public void UstawOVP(float volts) => WyslijZapis(82, (ushort)(volts * 100));
    public void UstawOCP(float amps) => WyslijZapis(83, (ushort)(amps * 100));
    public void WyslijRaw(byte adres, ushort wartosc) => WyslijZapis(adres, wartosc);

    private void WyslijZapis(byte adres, ushort wartosc)
    {
        _pendingWrites.Enqueue((adres, wartosc));
        if (adres < _registers.Length)
            _registers[adres] = wartosc;
        OnDataUpdated?.Invoke();
    }

    private void OproczKolejkeZapisow()
    {
        var s = _stream;
        if (s is null) return;
        while (_pendingWrites.TryDequeue(out var w))
        {
            byte[] req = new byte[11];
            req[0] = 0x01;
            req[1] = 0x10;
            req[2] = 0x00;
            req[3] = w.adres;
            req[4] = 0x00;
            req[5] = 0x01;
            req[6] = 0x02;
            req[7] = (byte)(w.wartosc >> 8);
            req[8] = (byte)(w.wartosc & 0xFF);
            byte[] crc = CalculateCRC(req, 0, 9);
            req[9] = crc[0];
            req[10] = crc[1];
            lock (_lock) { s.Write(req, 0, 11); }
            OnDebugData?.Invoke(">> " + BitConverter.ToString(req));
            // Czytaj echo odpowiedzi (FC=0x10)
            try
            {
                byte[] resp = new byte[8];
                int pos = 0;
                while (pos < 8)
                {
                    int n = s.Read(resp, pos, 8 - pos);
                    if (n == 0) break;
                    pos += n;
                }
                if (pos == 8 && resp[1] == 0x10 && resp[0] == 0x01)
                {
                    OnDataUpdated?.Invoke();
                    OnDebugData?.Invoke("<< " + BitConverter.ToString(resp) + " (echo)");
                }
            }
            catch { }
        }
    }

    public static byte[] CalculateCRC(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = 0; i < length; i++)
        {
            crc ^= data[offset + i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _stream?.Close();
        _client?.Close();
        _listener?.Stop();
        _serialPort?.Close();
    }
}
