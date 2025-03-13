using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host
{
    public class UpdateRequest
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public float UsedCPUPercent { get; set; }
        public float UsedRAMGigabytes { get; set; }
        public float TotalRAMGigabytes { get; set; }
    }
}
