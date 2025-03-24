using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Settings
{
    public class SettingsService
    {
        private SettingsModel _settings;
        private bool _settingsUpdated;
        private bool _settingsLocked;

        public SettingsModel Settings
        {
            get => _settings;
            private set => _settings = value;
        }

        public bool SettingsLocked => _settingsLocked;

        public event EventHandler Locked;

        public SettingsService()
        {
            Settings = new SettingsModel(false);
            _settingsLocked = true;
        }

        public SettingsModel GetUpdatedSettings()
        {
            if (_settingsUpdated)
            {
                _settingsUpdated = false;
                return Settings;
            }
            return null;
        }

        public void SetFromSettingsForm(SettingsModel settings)
        {
            Settings = settings;
            _settingsUpdated = true;
        }

        public void SetCurrentSettingsFromScreen(SettingsModel settings)
        {
            Settings = settings;
        }

        public void Unlock()
        {
            _settingsLocked = false;
        }

        public void Lock()
        {
            _settingsLocked = true;
            Locked?.Invoke(this, new EventArgs());
        }
    }
}
