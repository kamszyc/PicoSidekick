using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Media
{
    public class MediaReading
    {
        public bool IsMediaActive { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public bool IsPlaying { get; set; }
    }
}
