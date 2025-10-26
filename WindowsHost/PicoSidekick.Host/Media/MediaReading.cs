using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Media
{
    public class MediaReading
    {
        public bool IsMediaActive { get; init; }
        public string Artist { get; init; }
        public string Title { get; init; }
        public bool IsPlaying { get; init; }
    }
}
