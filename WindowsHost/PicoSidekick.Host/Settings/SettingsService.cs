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
        private bool _disableChanges;
        private Lock _lock = new Lock();

        public SettingsModel Settings
        {
            get => _settings;
            private set => _settings = value;
        }

        public bool SettingsLocked => _disableChanges;

        public event EventHandler ChangesDisabled;

        public SettingsService()
        {
            Settings = new SettingsModel(false, false);
            _disableChanges = true;
        }

        public SettingsModel GetUpdatedSettings()
        {
            lock (_lock)
            {
                if (_settingsUpdated)
                {
                    _settingsUpdated = false;
                    return Settings;
                }
                return null;
            }
        }

        public void SetFromSettingsForm(SettingsModel settings)
        {
            lock (_lock)
            {
                Settings = settings;
                _settingsUpdated = true;
            }
        }

        public void SetCurrentSettingsFromScreen(SettingsModel settings)
        {
            lock (_lock)
            {
                if (_settingsUpdated)
                    return;

                Settings = settings;
                _disableChanges = false;
            }
        }

        public void DisableChanges()
        {
            lock (_lock)
            {
                _disableChanges = true;
                ChangesDisabled?.Invoke(this, new EventArgs());
            }
        }
    }
}
