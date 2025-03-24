using PicoSidekick.Host.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Models
{
    public class UpdateRequest
    {
        public string Time { get; set; }
        public bool IsMediaActive { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public bool IsPlaying { get; set; }
        public float UsedCPUPercent { get; set; }
        public float UsedRAMGigabytes { get; set; }
        public float TotalRAMGigabytes { get; set; }
        public SettingsModel UpdatedSettings { get; set; }
    }
}
