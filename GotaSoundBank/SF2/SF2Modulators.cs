namespace GotaSoundBank.SF2
{
    public enum SF2Modulators : ushort
    {
        None = 0,
        NoteOnVelocity = 1,
        NoteOnKey = 2,
        PolyPressure = 0xA,
        ChnPressure = 0xD,
        PitchWheel = 0xE,
        PitchWheelSensivity = 0x10,
        Link = 127,
    }
}
