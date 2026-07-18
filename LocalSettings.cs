using System;
using System.IO;
using System.Text.Json;

namespace ZOYI
{
    internal static class LocalSettings
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "local_settings.json");

        private static bool _autoConnect;
        private static string _lastComPort = "";
        static LocalSettings()
        {
            Load();
        }

        public static bool AutoConnect
        {
            get => _autoConnect;
            set { _autoConnect = value; Save(); }
        }

        public static string LastComPort
        {
            get => _lastComPort;
            set { _lastComPort = value ?? ""; Save(); }
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("auto_connect", out var ac))
                        _autoConnect = ac.GetBoolean();
                    if (root.TryGetProperty("last_com_port", out var lp))
                        _lastComPort = lp.GetString() ?? "";
                }
                else
                {
                    _autoConnect = Properties.Settings.Default.auto_connect;
                    _lastComPort = Properties.Settings.Default.last_com_port ?? "";
                    Save();
                }
            }
            catch { }
        }

        private static void Save()
        {
            try
            {
                var obj = new { auto_connect = _autoConnect, last_com_port = _lastComPort };
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(FilePath, JsonSerializer.Serialize(obj, opts));
            }
            catch { }
        }
    }
}
