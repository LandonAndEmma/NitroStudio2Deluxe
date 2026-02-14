using System;
using Sanford.Multimedia.Midi;

namespace GotaSequenceLib.Playback
{
    public class MidiTrack : AbstractTrack
    {
        private readonly Sanford.Multimedia.Midi.Track _track = new Sanford.Multimedia.Midi.Track();
        private bool _tie = false;
        public override bool Tie
        {
            set
            {
                _tie = value;
                Message(
                    new ChannelMessage(
                        ChannelCommand.Controller,
                        Index,
                        (int)ControllerType.AllNotesOff
                    )
                );
            }
        }
        private bool _noteWait = true;
        public override bool NoteWait
        {
            set { _noteWait = value; }
        }
        private bool _portamento;
        public override bool Portamento
        {
            set { _portamento = value; }
        }
        private int _voice;
        public override int Voice
        {
            set { _voice = value; }
        }
        private byte _priority;
        public override byte Priority
        {
            set { _priority = value; }
        }
        private byte _volume;
        public override byte Volume
        {
            set { _volume = value; }
        }
        private byte _expression;
        public override byte Expression
        {
            set { _expression = value; }
        }
        private byte _lfoRange;
        public override byte LFORange
        {
            set { _lfoRange = value; }
        }
        private byte _pitchBendRange;
        public override byte PitchBendRange
        {
            set { _pitchBendRange = value; }
        }
        private byte _lfoSpeed;
        public override byte LFOSpeed
        {
            set { _lfoSpeed = value; }
        }
        private byte _lfoDepth;
        public override byte LFODepth
        {
            set { _lfoDepth = value; }
        }
        private ushort _lfoDelay;
        public override ushort LFODelay
        {
            set { _lfoDelay = value; }
        }
        private ushort _lfoPhase;
        public override ushort LFOPhase
        {
            set { _lfoPhase = value; }
        }
        private ushort _lfoDelayCount;
        public override ushort LFODelayCount
        {
            set { _lfoDelayCount = value; }
        }
        private LFOType _lfoType;
        public override LFOType LFOType
        {
            set { _lfoType = value; }
        }
        private sbyte _pitchBend;
        public override sbyte PitchBend
        {
            set { _pitchBend = value; }
        }
        private sbyte _panpot;
        public override sbyte Panpot
        {
            set { _panpot = value; }
        }
        private sbyte _transpose;
        public override sbyte Transpose
        {
            set { _transpose = value; }
        }
        private byte _attack;
        public override byte Attack
        {
            set { _attack = value; }
        }
        private byte _decay;
        public override byte Decay
        {
            set { _decay = value; }
        }
        private byte _sustain;
        public override byte Sustain
        {
            set { _sustain = value; }
        }
        private byte _hold;
        public override byte Hold
        {
            set { _hold = value; }
        }
        private byte _release;
        public override byte Release
        {
            set { _release = value; }
        }
        private byte _portamentoKey;
        public override byte PortamentoKey
        {
            set { _portamentoKey = value; }
        }
        private byte _portamentoTime;
        public override byte PortamentoTime
        {
            set { _portamentoTime = value; }
        }
        private short _sweepPitch;
        public override short SweepPitch
        {
            set { _sweepPitch = value; }
        }
        private int _bankNum;
        public override int BankNum
        {
            set { _bankNum = value; }
        }

        private void Message(ChannelMessage message)
        {
            _track.Insert((int)_player.ElapsedTicks, message);
        }

        public MidiTrack(byte idx, Player player)
            : base(idx, player) { }
    }
}
