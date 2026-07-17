using System.Text.Json;

namespace ZOYI
{
    public static class TranslationManager
    {
        private static readonly string _filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "translations.json");

        private static Dictionary<string, string> _translations = new();

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
            Save();
        }

        public static void Remove(string originalText)
        {
            if (_translations.Remove(originalText))
                Save();
        }

        public static void ApplyToForm(Form form)
        {
            ApplyToControl(form);
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
