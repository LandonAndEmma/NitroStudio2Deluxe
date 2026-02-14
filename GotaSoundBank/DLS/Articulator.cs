using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundBank.DLS
{
    public class Articulator
    {
        public List<Connection> Connections = new List<Connection>();
    }

    public class Connection
    {
        public SourceConnection SourceConnection;
        public ushort ControlConnection;
        public DestinationConnection DestinationConnection;
        public TransformConnection TransformConnection;
        public int Scale;
    }

    public enum SourceConnection : ushort
    {
        None,
        LFO,
        KeyOnVelocity,
        KeyNumber,
        EnvelopeGenerator1,
        EnvelopeGenerator2,
        PitchWheel,
        PolyPressure,
        ChannelPressure,
        Vibrato,
        Modulation = 0x81,
        ChannelVolume = 0x87,
        Pan = 0x8A,
        Expression,
        ChorusSend = 0xDB,
        ReverbSend = 0xDD,
        PitchBendRange = 0x100,
        FineTune,
        CoarseTune,
    }

    public enum DestinationConnection : ushort
    {
        None,
        Gain,
        Reserved,
        Pitch,
        Pan,
        KeyNumber,
        Left = 0x10,
        Right,
        Center,
        LFEChannel,
        LeftRear,
        RightRear,
        Chorus = 0x80,
        Reverb,
        LFOFrequency = 0x104,
        LFOStartDelayTime,
        EG1AttackTime = 0x206,
        EG1DecayTime,
        EG1Reserved,
        EG1ReleaseTime,
        EG1SustainLevel,
        EG1DelayTime,
        EG1HoldTime,
        EG1ShutDownTime,
        EG2AttackTime = 0x30A,
        EG2DecayTime,
        EG2Reserved,
        EG2ReleaseTime,
        EG2SustainLevel,
        EG2DelayTime,
        EG2HoldTime,
        FilterCutoffFrequency = 0x500,
        FilterResonance,
    }

    public enum TransformConnection : ushort
    {
        None,
        Concave,
        Convex,
        Switch,
    }
}
