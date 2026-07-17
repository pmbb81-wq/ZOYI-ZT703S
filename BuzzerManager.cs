using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ZOYI
{
    public class BuzzerManager : IDisposable
    {
        // Alarm (threshold-based, plays on ANY mode like Android reference)
        private WaveOutEvent? _alarmPlayer;
        private SignalGenerator? _alarmGen;
        private volatile bool _alarmPlaying;

        // Continuity/diode continuous beep
        private WaveOutEvent? _contPlayer;
        private SignalGenerator? _contGen;
        private volatile bool _contBeepPlaying;

        // Diode short beeps
        private Thread? _diodeThread;
        private volatile bool _diodePlaying;

        private volatile bool _disposed;

        public volatile bool AlarmEnabled;
        public double AlarmBelow;
        public double AlarmAbove;
        public int AlarmFrequency = 1000;

        public volatile bool DiodeBeepEnabled;
        public int DiodeFrequency = 2000;
        public int DiodeInterval = 500;
        public int ContinuityThreshold = 30;
        public int DiodeShortThreshold = 1;

        private bool _alarmTriggered;
        private long _lastDiodePatternTime;

        public bool IsAlarmTriggered => _alarmTriggered;
        public bool IsContBeepPlaying => _contBeepPlaying;

        public void CheckAlarm(string? val)
        {
            if (!AlarmEnabled)
            {
                if (_alarmTriggered) { _alarmTriggered = false; StopAlarmSound(); }
                return;
            }
            if (string.IsNullOrEmpty(val) || val == "OL")
            {
                if (_alarmTriggered) { _alarmTriggered = false; StopAlarmSound(); }
                return;
            }

            try
            {
                double num = double.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                if (Math.Abs(num) < 0.0001)
                {
                    if (_alarmTriggered) { _alarmTriggered = false; StopAlarmSound(); }
                    return;
                }
                bool triggered = (AlarmBelow > 0 && num < AlarmBelow) || (AlarmAbove > 0 && num > AlarmAbove);
                if (triggered)
                {
                    if (!_alarmTriggered)
                    {
                        _alarmTriggered = true;
                        StartAlarmSound();
                    }
                }
                else
                {
                    if (_alarmTriggered)
                    {
                        _alarmTriggered = false;
                        StopAlarmSound();
                    }
                }
            }
            catch
            {
                if (_alarmTriggered) { _alarmTriggered = false; StopAlarmSound(); }
            }
        }

        public void CheckContinuityBeep(FrameDecoder fd)
        {
            if (!DiodeBeepEnabled)
            {
                StopDiodeContBeep();
                _diodePlaying = false;
                _lastDiodePatternTime = 0;
                return;
            }

            if (fd.IsContinuity)
            {
                try
                {
                    if (fd.BaseValue.HasValue)
                    {
                        double rv = Math.Abs(fd.BaseValue.Value);
                        if (rv <= ContinuityThreshold)
                        {
                            if (!_contBeepPlaying) StartDiodeContBeep();
                        }
                        else
                        {
                            StopDiodeContBeep();
                        }
                    }
                    else
                    {
                        StopDiodeContBeep();
                    }
                    _diodePlaying = false;
                    _lastDiodePatternTime = 0;
                }
                catch { }
            }
            else if (fd.IsDiode)
            {
                try
                {
                    if (fd.BaseValue.HasValue)
                    {
                        double dv = Math.Abs(fd.BaseValue.Value);
                        bool isShort = dv < (DiodeShortThreshold / 1000.0);
                        bool inDiodeRange = dv >= 0.15 && dv <= 0.8;

                        if (isShort)
                        {
                            if (!_contBeepPlaying) StartDiodeContBeep();
                            _diodePlaying = false;
                            _lastDiodePatternTime = 0;
                        }
                        else if (inDiodeRange)
                        {
                            StopDiodeContBeep();
                            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            if (!_diodePlaying && _lastDiodePatternTime == 0)
                            {
                                _lastDiodePatternTime = now;
                                PlayDiodeBeep(dv >= 0.5 ? 1 : 2);
                            }
                            else if (!_diodePlaying && now - _lastDiodePatternTime >= DiodeInterval)
                            {
                                _lastDiodePatternTime = now;
                                PlayDiodeBeep(dv >= 0.5 ? 1 : 2);
                            }
                        }
                        else
                        {
                            StopDiodeContBeep();
                            _diodePlaying = false;
                            _lastDiodePatternTime = 0;
                        }
                    }
                }
                catch { }
            }
            else
            {
                StopDiodeContBeep();
                _diodePlaying = false;
                _lastDiodePatternTime = 0;
            }
        }

        private void StartAlarmSound()
        {
            if (_alarmPlaying) return;
            _alarmPlaying = true;
            try
            {
                _alarmGen = new SignalGenerator
                {
                    Gain = 0.5f,
                    Frequency = AlarmFrequency,
                    Type = SignalGeneratorType.Sin
                };
                _alarmPlayer = new WaveOutEvent();
                _alarmPlayer.Init(_alarmGen);
                _alarmPlayer.Play();
            }
            catch
            {
                _alarmPlaying = false;
                StopAlarmSound();
            }
        }

        public void StopAlarmSound()
        {
            _alarmPlaying = false;
            try { _alarmPlayer?.Stop(); } catch { }
            try { _alarmPlayer?.Dispose(); } catch { }
            _alarmPlayer = null;
            _alarmGen = null;
        }

        private void StartDiodeContBeep()
        {
            if (_contBeepPlaying) return;
            _contBeepPlaying = true;
            try
            {
                _contGen = new SignalGenerator
                {
                    Gain = 0.5f,
                    Frequency = DiodeFrequency,
                    Type = SignalGeneratorType.Sin
                };
                _contPlayer = new WaveOutEvent();
                _contPlayer.Init(_contGen);
                _contPlayer.Play();
            }
            catch
            {
                _contBeepPlaying = false;
                StopDiodeContBeep();
            }
        }

        private void StopDiodeContBeep()
        {
            _contBeepPlaying = false;
            try { _contPlayer?.Stop(); } catch { }
            try { _contPlayer?.Dispose(); } catch { }
            _contPlayer = null;
            _contGen = null;
        }

        private void PlayDiodeBeep(int count)
        {
            if (_diodePlaying || count < 1) return;
            _diodePlaying = true;
            _diodeThread = new Thread(() =>
            {
                try
                {
                    int sampleRate = 44100;
                    int freq = DiodeFrequency;
                    int beepMs = 60;
                    int gapMs = 100;
                    int totalMs = count * beepMs + (count - 1) * gapMs;
                    int totalSamples = sampleRate * totalMs / 1000;
                    short[] buf = new short[totalSamples];

                    for (int n = 0; n < count; n++)
                    {
                        int start = sampleRate * (n * (beepMs + gapMs)) / 1000;
                        int end = start + sampleRate * beepMs / 1000;
                        for (int i = start; i < end && i < totalSamples; i++)
                        {
                            double angle = 2.0 * Math.PI * freq * i / sampleRate;
                            buf[i] = (short)(short.MaxValue * 0.6 * Math.Sin(angle));
                        }
                    }

                    var waveProvider = new ShortWaveProvider(sampleRate, 1);
                    waveProvider.Buffer = buf;
                    using var player = new WaveOutEvent();
                    player.Init(waveProvider);
                    player.Play();
                    Thread.Sleep(totalMs);
                }
                catch { }
                _diodePlaying = false;
            }) { IsBackground = true };
            _diodeThread.Start();
        }

        public void StopAll()
        {
            StopAlarmSound();
            StopDiodeContBeep();
            _diodePlaying = false;
            _lastDiodePatternTime = 0;
            _alarmTriggered = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopAll();
        }

        private class ShortWaveProvider : IWaveProvider
        {
            private int _position;
            public short[] Buffer { get; set; } = Array.Empty<short>();
            public WaveFormat WaveFormat { get; }

            public ShortWaveProvider(int sampleRate, int channels)
            {
                WaveFormat = new WaveFormat(sampleRate, 16, channels);
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                int samplesNeeded = count / 2;
                int samplesAvailable = Buffer.Length - _position;
                int samplesToCopy = Math.Min(samplesNeeded, samplesAvailable);

                for (int i = 0; i < samplesToCopy; i++)
                {
                    short sample = _position + i < Buffer.Length ? Buffer[_position + i] : (short)0;
                    buffer[offset + i * 2] = (byte)(sample & 0xFF);
                    buffer[offset + i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
                }

                _position += samplesToCopy;
                return samplesToCopy * 2;
            }
        }
    }
}
