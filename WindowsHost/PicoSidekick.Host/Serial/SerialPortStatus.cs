using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Serial
{
    public record SerialPortStatus(bool IsConnected, string PortName);
}
