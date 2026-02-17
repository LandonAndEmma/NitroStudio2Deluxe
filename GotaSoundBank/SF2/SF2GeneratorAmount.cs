using System.Runtime.InteropServices;

namespace GotaSoundBank.SF2
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SF2GeneratorAmount
    {
        [FieldOffset(0)]
        public byte LowByte;

        [FieldOffset(1)]
        public byte HighByte;

        [FieldOffset(0)]
        public short Amount;

        [FieldOffset(0)]
        public ushort UAmount;

        public override string ToString()
        {
            return $"BLo = {LowByte}, BHi = {HighByte}, Sh = {Amount}, U = {UAmount}";
        }
    }
}
