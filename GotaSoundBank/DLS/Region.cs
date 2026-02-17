using System.Collections.Generic;

namespace GotaSoundBank.DLS
{
    public class Region
    {
        public ushort NoteLow = 0;
        public ushort NoteHigh = 127;
        public ushort VelocityLow = 0;
        public ushort VelocityHigh = 127;
        public bool DoublePlayback = true;
        public byte KeyGroup;
        public ushort Layer;
        public byte RootNote = 60;
        public short Tuning;
        public int Gain;
        public bool NoTruncation = true;
        public bool NoCompression;
        public bool Loops;
        public bool LoopAndRelease;
        public uint LoopStart;
        public uint LoopLength;
        public bool PhaseMaster;
        public bool MultiChannel;
        public ushort PhaseGroup;
        public uint ChannelFlags;
        public uint WaveId;
        public List<Articulator> Articulators = [];
    }
}
