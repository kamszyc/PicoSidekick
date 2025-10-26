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
        public string Time { get; init; }
        public bool IsMediaActive { get; init; }
        public string Artist { get; init; }
        public string Title { get; init; }
        public bool IsPlaying { get; init; }
        public float UsedCPUPercent { get; init; }
        public float UsedRAMGigabytes { get; init; }
        public float TotalRAMGigabytes { get; init; }
        public SettingsModel UpdatedSettings { get; init; }
    }
}
