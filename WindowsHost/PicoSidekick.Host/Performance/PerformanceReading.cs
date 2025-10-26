using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Performance
{
    public record PerformanceReading(float Cpu, float UsedRamInGigabytes);
}
