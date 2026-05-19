using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ZOYI
{
    public class ShortcutManager
    {
        public enum ActionId
        {
            ToggleStandardPanel,
            ToggleAdvancedPanel,
            ClearLog,
            SaveCSV,
            ToggleChartPause,
            ClearChart,
            MinimizeWindow,
            ToggleTTS,
            CycleTimeWindow,
            OpenShortcutConfig
        }

        public class ShortcutEntry
        {
            public ActionId Id;
            public string DisplayName;
            public Keys Keys;

            public ShortcutEntry(ActionId id, string displayName, Keys defaultKeys)
            {
                Id = id;
                DisplayName = displayName;
                Keys = defaultKeys;
            }
        }

        private readonly Dictionary<ActionId, ShortcutEntry> shortcuts = new Dictionary<ActionId, ShortcutEntry>();
        private readonly Dictionary<ActionId, Action> actions = new Dictionary<ActionId, Action>();

        public ShortcutManager()
        {
            shortcuts[ActionId.ToggleStandardPanel] = new ShortcutEntry(ActionId.ToggleStandardPanel, "Przełącz panel standardowy", Keys.F1);
            shortcuts[ActionId.ToggleAdvancedPanel] = new ShortcutEntry(ActionId.ToggleAdvancedPanel, "Przełącz panel zaawansowany", Keys.F2);
            shortcuts[ActionId.ClearLog] = new ShortcutEntry(ActionId.ClearLog, "Wyczyść log", Keys.F3);
            shortcuts[ActionId.SaveCSV] = new ShortcutEntry(ActionId.SaveCSV, "Zapisz CSV", Keys.F4);
            shortcuts[ActionId.ToggleChartPause] = new ShortcutEntry(ActionId.ToggleChartPause, "Pauza/Wznów wykres", Keys.F5);
            shortcuts[ActionId.ClearChart] = new ShortcutEntry(ActionId.ClearChart, "Wyczyść wykres", Keys.F6);
            shortcuts[ActionId.MinimizeWindow] = new ShortcutEntry(ActionId.MinimizeWindow, "Minimalizuj okno", Keys.F7);
            shortcuts[ActionId.ToggleTTS] = new ShortcutEntry(ActionId.ToggleTTS, "Przełącz TTS", Keys.F8);
            shortcuts[ActionId.CycleTimeWindow] = new ShortcutEntry(ActionId.CycleTimeWindow, "Zmień okno czasowe", Keys.F9);
            shortcuts[ActionId.OpenShortcutConfig] = new ShortcutEntry(ActionId.OpenShortcutConfig, "Konfiguracja skrótów", Keys.F12);

            LoadFromSettings();
        }

        public void RegisterAction(ActionId id, Action action)
        {
            actions[id] = action;
        }

        public bool ProcessKey(Keys keyData)
        {
            foreach (var entry in shortcuts.Values)
            {
                if (entry.Keys == keyData)
                {
                    if (actions.ContainsKey(entry.Id))
                    {
                        actions[entry.Id]();
                        return true;
                    }
                }
            }
            return false;
        }

        public List<ShortcutEntry> GetShortcuts()
        {
            return new List<ShortcutEntry>(shortcuts.Values);
        }

        public void SetShortcut(ActionId id, Keys keys)
        {
            if (shortcuts.ContainsKey(id))
            {
                shortcuts[id].Keys = keys;
            }
        }

        public void SaveToSettings()
        {
            var settings = Properties.Settings.Default;
            foreach (var entry in shortcuts.Values)
            {
                string propName = "shortcut_" + entry.Id.ToString();
                if (settings.Properties[propName] != null)
                {
                    settings[propName] = (int)entry.Keys;
                }
            }
            settings.Save();
        }

        private void LoadFromSettings()
        {
            var settings = Properties.Settings.Default;
            foreach (var entry in shortcuts.Values)
            {
                string propName = "shortcut_" + entry.Id.ToString();
                if (settings.Properties[propName] != null)
                {
                    int savedValue = (int)settings[propName];
                    if (savedValue != 0)
                    {
                        entry.Keys = (Keys)savedValue;
                    }
                }
            }
        }

        public static string KeysToString(Keys keys)
        {
            if (keys == Keys.None)
                return "Brak";

            var parts = new List<string>();
            if ((keys & Keys.Control) == Keys.Control)
                parts.Add("Ctrl");
            if ((keys & Keys.Alt) == Keys.Alt)
                parts.Add("Alt");
            if ((keys & Keys.Shift) == Keys.Shift)
                parts.Add("Shift");

            Keys key = keys & Keys.KeyCode;
            if (key != Keys.None)
                parts.Add(key.ToString());

            return string.Join(" + ", parts);
        }
    }
}
