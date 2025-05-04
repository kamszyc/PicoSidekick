using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Models
{
    public class ScreenCommand
    {
        private const string ShutdownCommand = "shutdown";
        private const string SettingsCommand = "settings";

        public string Command { get; set; }

        public bool DevModeEnabled { get; set; }

        public int Brightness { get; set; }

        public bool DisplayRotated { get; set; }

        public bool IsShutdown()
        {
            return Command == ShutdownCommand;
        }

        public bool IsSettings()
        {
            return Command == SettingsCommand;
        }
    }
}
