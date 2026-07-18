using System.Text.Json;

namespace ZOYI
{
    public static class TranslationManager
    {
        private static readonly string _filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "translations.json");

        private static Dictionary<string, string> _translations = new();
        private static Dictionary<string, string> _reverseLookup = new();

        public static IReadOnlyDictionary<string, string> Translations => _translations;

        static TranslationManager()
        {
            Load();
        }

        public static string? Get(string originalText)
        {
            if (string.IsNullOrEmpty(originalText)) return null;
            _translations.TryGetValue(originalText, out var translated);
            return translated;
        }

        public static string GetOriginal(string currentText)
        {
            if (string.IsNullOrEmpty(currentText)) return currentText;
            if (_reverseLookup.TryGetValue(currentText, out var original))
                return original;
            return currentText;
        }

        public static void Set(string originalText, string translatedText)
        {
            if (string.IsNullOrWhiteSpace(originalText)) return;

            if (string.IsNullOrWhiteSpace(translatedText))
            {
                _translations.Remove(originalText);
            }
            else
            {
                _translations[originalText] = translatedText;
            }
            RebuildReverseLookup();
            Save();
        }

        public static void Remove(string originalText)
        {
            if (_translations.Remove(originalText))
            {
                RebuildReverseLookup();
                Save();
            }
        }

        public static void ApplyToForm(Form form)
        {
            _reverseLookup.Clear();
            ApplyToControl(form);
            RebuildReverseLookup();
        }

        private static void ApplyToControl(Control control)
        {
            if (control.Text != null)
            {
                string? translated = Get(control.Text);
                if (translated != null)
                {
                    control.Text = translated;
                }
            }

            if (control.ContextMenuStrip != null)
            {
                foreach (ToolStripItem item in control.ContextMenuStrip.Items)
                {
                    if (item.Text != null)
                    {
                        string? translated = Get(item.Text);
                        if (translated != null)
                            item.Text = translated;
                    }
                }
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child);
            }
        }

        private static void RebuildReverseLookup()
        {
            _reverseLookup.Clear();
            foreach (var kv in _translations)
            {
                _reverseLookup[kv.Value] = kv.Key;
            }
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                                    ?? new Dictionary<string, string>();
                }
            }
            catch
            {
                _translations = new Dictionary<string, string>();
            }
            RebuildReverseLookup();
        }

        private static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                string json = JsonSerializer.Serialize(_translations, options);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
            }
        }
    }
}
