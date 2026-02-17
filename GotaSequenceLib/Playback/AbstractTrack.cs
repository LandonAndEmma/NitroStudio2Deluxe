namespace GotaSequenceLib.Playback
{
    public abstract class AbstractTrack
    {
        public readonly byte Index;
        protected readonly Player _player;
        public bool Allocated;
        public bool Enabled;
        public bool Stopped;
        public int CurEvent;
        public bool VariableFlag;
        public int Rest;
        public int[] CallStack = new int[3];
        public byte[] CallStackLoops = new byte[3];
        public byte CallStackDepth;
        public bool WaitingForNoteToFinishBeforeContinuingXD;
        public bool NoteDown;
        public abstract bool Tie { set; }
        public abstract bool NoteWait { set; }
        public abstract bool Portamento { set; }
        public abstract int Voice { set; }
        public abstract byte Priority { set; }
        public abstract byte Volume { set; }
        public abstract byte Expression { set; }
        public abstract byte LFORange { set; }
        public abstract byte PitchBendRange { set; }
        public abstract byte LFOSpeed { set; }
        public abstract byte LFODepth { set; }
        public abstract ushort LFODelay { set; }
        public abstract ushort LFOPhase { set; }
        public abstract ushort LFODelayCount { set; }
        public abstract LFOType LFOType { set; }
        public abstract sbyte PitchBend { set; }
        public abstract sbyte Panpot { set; }
        public abstract sbyte Transpose { set; }
        public abstract byte Attack { set; }
        public abstract byte Decay { set; }
        public abstract byte Sustain { set; }
        public abstract byte Hold { set; }
        public abstract byte Release { set; }
        public abstract byte PortamentoKey { set; }
        public abstract byte PortamentoTime { set; }
        public abstract short SweepPitch { set; }
        public abstract int BankNum { set; }

        public class TrackVars
        {
            private readonly Player _player;
            private readonly short[] _trackVars = new short[0x10];
            public short this[int i]
            {
                get => i < 0x20 ? _player.Vars[i] : _trackVars[i - 0x20];
                set
                {
                    if (i < 0x20)
                    {
                        _player.Vars[i] = value;
                    }
                    else
                    {
                        _trackVars[i - 0x20] = value;
                    }
                }
            }

            internal TrackVars(Player player)
            {
                _player = player;
            }
        }

        public readonly TrackVars Vars;

        protected AbstractTrack(byte idx, Player player)
        {
            Index = idx;
            _player = player;
            Vars = new TrackVars(player);
        }
    }
}
