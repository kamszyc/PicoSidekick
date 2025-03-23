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

        public string Command { get; set; }

        public bool IsShutdown()
        {
            return Command == ShutdownCommand;
        }
    }
}
