using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Settings
{
    public record SettingsModel(bool DevModeEnabled, bool RestartInUf2Mode)
    {
    }
}
