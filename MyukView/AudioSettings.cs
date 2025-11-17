using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyukView
{
    public class AudioSettings
    {
        public string Codec { get; set; } = "aac";
        public int Bitrate { get; set; } = 192;
        public string Extension { get; set; } = ".m4a";
    }
}
