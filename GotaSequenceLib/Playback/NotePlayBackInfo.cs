using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSequenceLib.Playback
{
    public class NotePlayBackInfo
    {
        public int WaveId;
        public int WarId;
        public InstrumentType InstrumentType;
        public byte Attack = 127;
        public byte Decay = 127;
        public byte Sustain = 127;
        public byte Hold = 0;
        public byte Release = 127;
        public byte BaseKey = 60;
        public byte Pan = 64;
        public sbyte SurroundPan;
        public byte Volume = 127;
        public byte KeyGroup = 0;
        public float Tune = 1f;
        public bool PercussionMode = false;
        public bool IsLinearInterpolation = false;
    }
}
