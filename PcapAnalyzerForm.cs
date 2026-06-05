using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ZOYI;

public partial class PcapAnalyzerForm : Form
{
    private readonly TextBox _logBox;
    private readonly Button _openBtn;
    private readonly Button _stopBtn;
    private long _packetCount;
    private volatile bool _stopRequested;

    public PcapAnalyzerForm()
    {
        Text = "PCAP Analyzer - Modbus RTU over TCP";
        Size = new(1200, 800);

        _openBtn = new Button { Text = "Otworz PCAP...", Location = new(12, 12), Size = new(120, 32) };
        _openBtn.Click += OpenClick;

        _stopBtn = new Button { Text = "STOP", Location = new(140, 12), Size = new(90, 32), BackColor = Color.DarkRed, ForeColor = Color.White, Font = new("Segoe UI", 9, FontStyle.Bold), Enabled = false };
        _stopBtn.Click += (_, _) => { _stopRequested = true; Log("--- PRZERWANO PRZEZ UZYTKOWNIKA ---"); };

        _logBox = new TextBox
        {
            Multiline = true,
            Location = new(12, 52),
            Size = new(1150, 690),
            ReadOnly = false,
            Font = new("Consolas", 9),
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
        };

        Controls.Add(_openBtn);
        Controls.Add(_stopBtn);
        Controls.Add(_logBox);

        Label lbl = new()
        {
            Text = "Wybrano 0 plikow. Kliknij Otworz i wybierz plik .pcap lub .pcapng",
            Location = new(240, 16),
            Size = new(500, 24)
        };
        Controls.Add(lbl);
        _openBtn.Tag = lbl;

        Shown += (_, _) => { _logBox.Select(); };
    }

    private void OpenClick(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "PCAP files (*.pcap;*.pcapng)|*.pcap;*.pcapng|All files (*.*)|*.*",
            Title = "Wybierz plik przechwyconego ruchu Wireshark"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var lbl = (Label)_openBtn.Tag!;
        lbl.Text = $"Plik: {Path.GetFileName(dlg.FileName)}";
        _logBox.Clear();
        _packetCount = 0;
        _stopRequested = false;
        _stopBtn.Enabled = true;

        try
        {
            byte[] data = File.ReadAllBytes(dlg.FileName);
            if (data.Length < 4) { Log("BLAD: Plik za maly."); return; }

            uint magic = ReadU32(data, 0);
            Log($"Format: magic = 0x{magic:X8}, rozmiar pliku = {data.Length} B");

            if (magic == 0x0A0D0D0A)
                ParsePcapNg(data);
            else if (magic == 0xA1B2C3D4 || magic == 0xD4C3B2A1)
                ParsePcap(data, magic);
            else
                Log("BLAD: Nieznany format pliku (magic = 0x" + magic.ToString("X8") + ")");
        }
        catch (Exception ex)
        {
            Log($"BLAD: {ex.Message}");
        }
        finally
        {
            _stopBtn.Enabled = false;
        }

        Log($"\n--- Przeanalizowano {_packetCount} pakietow ---");
    }

    // ─── PCAP format ──────────────────────────────────────────────
    private void ParsePcap(byte[] data, uint magic)
    {
        bool swap = magic == 0xD4C3B2A1;
        int network = (int)ReadU32(data, 20, swap);
        Log($"  PCAP: network (link type) = {network}");
        int off = 24; // skip global header

        while (off + 16 <= data.Length)
        {
            if (_stopRequested) break;

            int inclLen = (int)ReadU32(data, off + 8, swap);
            if (inclLen < 0 || inclLen > 65535) break;
            if (off + 16 + inclLen > data.Length) break;

            long tsSec = ReadU32(data, off, swap);
            long tsUsec = ReadU32(data, off + 4, swap);
            DateTime ts = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(tsSec).AddMilliseconds(tsUsec / 1000.0);

            AnalyzePacket(data, off + 16, inclLen, ts);
            off += 16 + inclLen;
            _packetCount++;
        }
    }

    // ─── PCAPNG format ────────────────────────────────────────────
    private void ParsePcapNg(byte[] data)
    {
        int off = 0;
        int totalBlocks = 0, epbCount = 0, spbCount = 0, idbCount = 0;
        while (off + 8 <= data.Length)
        {
            uint blockType = ReadU32(data, off, swap: true);
            int blockLen = (int)ReadU32(data, off + 4, swap: true);
            if (blockLen < 12 || blockLen > data.Length - off) { Log($"  Przerwano na off={off}, blockLen={blockLen}"); break; }

            totalBlocks++;

            if (_stopRequested) break;

            if (totalBlocks <= 5)
                Log($"  Blok #{totalBlocks}: typ=0x{blockType:X8} len={blockLen}");

            if (blockType == 0x00000006) // Enhanced Packet Block
            {
                epbCount++;
                if (off + 28 <= data.Length)
                {
                    long tsHigh = ReadU32(data, off + 12, swap: true);
                    long tsLow = ReadU32(data, off + 16, swap: true);
                    ulong tsRaw = ((ulong)tsHigh << 32) | (ulong)tsLow;
                    int capLen = (int)ReadU32(data, off + 20, swap: true);

                    if (capLen > 0 && capLen < 65536 && off + 28 + capLen <= data.Length)
                    {
                        long ms = (long)(tsRaw / 1000);
                        DateTime ts;
                        try { ts = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime; }
                        catch { ts = DateTime.UtcNow; }
                        AnalyzePacket(data, off + 28, capLen, ts);
                        _packetCount++;
                    }
                }
            }
            else if (blockType == 0x00000003) // Simple Packet Block
            {
                spbCount++;
                int origLen = (int)ReadU32(data, off + 8, swap: true);
                int capLen = blockLen - 16; // SPB: data starts at off+12, padding to 4, trailing blockLen
                if (capLen > 0 && capLen <= origLen && off + 12 + capLen <= data.Length)
                {
                    DateTime ts = DateTime.UtcNow;
                    AnalyzePacket(data, off + 12, capLen, ts);
                    _packetCount++;
                }
            }
            else             if (blockType == 0x00000001) // Interface Description Block
            {
                idbCount++;
                if (off + 16 <= data.Length)
                {
                    int linkType = ReadU16(data, off + 8, swap: true);
                    int snapLen = (int)ReadU32(data, off + 12, swap: true);
                    Log($"  IDB: snaplen={snapLen}, linkType={linkType}");
                }
            }

            off += blockLen;
        }
        Log($"  Razem blokow: {totalBlocks}, EPB={epbCount}, SPB={spbCount}, IDB={idbCount}");
    }

    // ─── Ethernet / IP / TCP / Modbus ─────────────────────────────
    private void AnalyzePacket(byte[] data, int off, int len, DateTime ts)
    {
        // Try to locate IP header - handles Ethernet, raw IP, SLL, etc.
        int ipStart = FindIpHeader(data, off, len);
        if (ipStart < 0) return;

        int ipOff = ipStart;
        if (ipOff + 20 > off + len) return;
        byte ipVerIhl = data[ipOff];
        int ipHdrLen = (ipVerIhl & 0x0F) * 4;
        if (ipHdrLen < 20) return;
        byte ipProto = data[ipOff + 9];
        if (ipProto != 6 && ipProto != 17) return; // TCP or UDP

        // Try TCP first
        if (ipProto == 6)
        {
            int tcpOff = ipOff + ipHdrLen;
            if (tcpOff + 20 > off + len) return;
            byte tcpOffFlags = data[tcpOff + 12];
            int tcpHdrLen = ((tcpOffFlags >> 4) & 0x0F) * 4;
            if (tcpHdrLen < 20) return;

            int payloadOff = tcpOff + tcpHdrLen;
            int payloadLen = (off + len) - payloadOff;
            if (payloadLen < 2) return;

            int srcPort = (data[tcpOff] << 8) | data[tcpOff + 1];
            int dstPort = (data[tcpOff + 2] << 8) | data[tcpOff + 3];

            string srcIp = $"{data[ipOff + 12]}.{data[ipOff + 13]}.{data[ipOff + 14]}.{data[ipOff + 15]}";
            string dstIp = $"{data[ipOff + 16]}.{data[ipOff + 17]}.{data[ipOff + 18]}.{data[ipOff + 19]}";

            ParseModbusFrames(data, payloadOff, payloadLen, ts, srcIp, srcPort, dstIp, dstPort);
            return;
        }

        // UDP fallback - scan entire packet for Modbus RTU
        ParseModbusFrames(data, off, len, ts, "?", 0, "?", 0);
    }

    private static int FindIpHeader(byte[] data, int off, int len)
    {
        int end = off + len;
        // Try Ethernet (off+12 = EtherType, 0x0800 = IPv4)
        if (off + 14 + 20 <= end)
        {
            ushort ethType = (ushort)((data[off + 12] << 8) | data[off + 13]);
            if (ethType == 0x0800 && (data[off + 14] >> 4) == 4)
                return off + 14;
            if (ethType == 0x86DD && (data[off + 14] >> 4) == 6)
                return off + 14; // IPv6, skip later
        }

        // Try raw IP (no link header)
        if (off + 20 <= end && (data[off] >> 4) == 4)
            return off;

        // Try Linux SLL (off+14..15 = protocol)
        if (off + 16 + 20 <= end)
        {
            ushort sllType = (ushort)((data[off + 14] << 8) | data[off + 15]);
            if (sllType == 0x0800 && (data[off + 16] >> 4) == 4)
                return off + 16;
        }

        return -1;
    }

    // ─── Modbus RTU frame detection ──────────────────────────────
    private void ParseModbusFrames(byte[] data, int off, int len, DateTime ts,
        string srcIp, int srcPort, string dstIp, int dstPort)
    {
        int pos = off;
        int end = off + len;

        while (pos + 4 <= end)
        {
            if (data[pos] != 0x01) { pos++; continue; }
            byte func = data[pos + 1];

            int frameLen = GuessFrameLen(data, pos, end - pos);
            if (frameLen <= 0) { pos++; continue; }

            string hex = BitConverter.ToString(data, pos, frameLen);
            string desc = DescribeFrame(data, pos, frameLen);

            string time = ts.ToLocalTime().ToString("HH:mm:ss.fff");
            string dir = srcPort is 502 or 8080
                ? $"RD60xx → APP  ({srcIp}:{srcPort} → {dstIp}:{dstPort})"
                : $"APP → RD60xx  ({srcIp}:{srcPort} → {dstIp}:{dstPort})";

            byte[] crc = CalculateCRC(data, pos, frameLen - 2);
            bool crcOk = crc[0] == data[pos + frameLen - 2] && crc[1] == data[pos + frameLen - 1];

            Log($"[{time}] {dir}\n  HEX: {hex}\n  CRC: {(crcOk ? "OK" : "BLEDNY")} | {desc}\n");

            pos += frameLen;
        }
    }

    private static int GuessFrameLen(byte[] data, int off, int max)
    {
        byte func = data[off + 1];
        if ((func & 0x80) != 0) return 5; // exception

        return func switch
        {
            0x01 or 0x02 => 3 + data[off + 2] + 2,
            0x03 or 0x04 => 3 + data[off + 2] + 2,
            0x05 => 8,
            0x06 => 8,
            0x0F or 0x10 => 8, // response is 8 bytes
            _ when func >= 0x41 && func <= 0x7E => max >= 8 ? 8 : 0,
            _ => 8 // assume 8 for unknown
        };
    }

    private static string DescribeFrame(byte[] data, int off, int len)
    {
        byte func = data[off + 1];
        if ((func & 0x80) != 0)
            return $"BLAD: func=0x{func & 0x7F:X2}, code={data[off + 2]} ({ModbusErrDesc(data[off + 2])})";

        ushort addr = (ushort)((data[off + 2] << 8) | data[off + 3]);

        return func switch
        {
            0x03 or 0x04 => DescribeRead(func, data, off, len),
            0x06 => DescribeWrite06(data, off),
            0x10 => DescribeWrite10(data, off),
            _ => $"Funkcja 0x{func:X2}, adres 0x{addr:X4}"
        };
    }

    private static string DescribeRead(byte func, byte[] data, int off, int len)
    {
        byte count = data[off + 2];
        string regs = "";
        for (int i = 0; i < count / 2 && 3 + i * 2 + 2 <= len - 2; i++)
        {
            ushort val = (ushort)((data[off + 3 + i * 2] << 8) | data[off + 3 + i * 2 + 1]);
            if (i <= 20)
                regs += $"  reg[{i}] = {val} (0x{val:X4}){DescribeReg(i, val)}\n";
        }
        int startAddr = 0; // always 0 for this device
        return $"{(func == 0x03 ? "Holding" : "Input")} Registers [{startAddr}..{startAddr + count / 2 - 1}] ({count} bajtow):\n{regs}";
    }

    private static string DescribeWrite06(byte[] data, int off)
    {
        ushort addr = (ushort)((data[off + 2] << 8) | data[off + 3]);
        ushort val = (ushort)((data[off + 4] << 8) | data[off + 5]);
        return $"Write Single (FC=0x06): reg 0x{addr:X2} ({addr}) = {val} (0x{val:X4}){DescribeReg(addr, val)}";
    }

    private static string DescribeWrite10(byte[] data, int off)
    {
        ushort addr = (ushort)((data[off + 2] << 8) | data[off + 3]);
        ushort count = (ushort)((data[off + 4] << 8) | data[off + 5]);
        return $"Write Multiple (FC=0x10): reg 0x{addr:X2} x {count}";
    }

    private static string DescribeReg(int addr, ushort val)
    {
        return addr switch
        {
            8 => $"  ← Vset = {val / 100f:F2} V",
            9 => $"  ← Iset = {val / 100f:F2} A",
            10 => $"  ← Vout = {val / 100f:F2} V",
            11 => $"  ← Iout = {val / 100f:F2} A",
            12 or 13 => "",
            18 => $"  ← Output = {(val == 1 ? "ON" : "OFF")}",
            _ => ""
        };
    }

    private static string ModbusErrDesc(byte code) => code switch
    {
        0x01 => "Illegal Function",
        0x02 => "Illegal Data Address",
        0x03 => "Illegal Data Value",
        0x04 => "Slave Device Failure",
        _ => $"Unknown (0x{code:X2})"
    };

    // ─── CRC ──────────────────────────────────────────────────────
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

    // ─── Helpers ──────────────────────────────────────────────────
    private static ushort ReadU16(byte[] d, int off, bool swap = false)
    {
        if (swap)
            return (ushort)((d[off + 1] << 8) | d[off]);
        return (ushort)((d[off] << 8) | d[off + 1]);
    }

    private static uint ReadU32(byte[] d, int off, bool swap = false)
    {
        if (swap)
            return (uint)((d[off + 3] << 24) | (d[off + 2] << 16) | (d[off + 1] << 8) | d[off]);
        return (uint)((d[off] << 24) | (d[off + 1] << 16) | (d[off + 2] << 8) | d[off + 3]);
    }

    private void Log(string msg)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            try { Invoke(() => Log(msg)); } catch { }
            return;
        }
        _logBox.AppendText(msg + "\r\n");
    }
}
