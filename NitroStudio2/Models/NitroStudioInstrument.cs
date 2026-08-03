using GotaSoundIO.IO;
using NitroFileLoader.Instrument;

namespace NitroStudio2.Models
{
    /// <summary>
    /// The .nist single-instrument file. Ported unchanged from the WinForms project, except that
    /// an empty instrument now leaves <see cref="Inst"/> null instead of showing a message box.
    /// </summary>
    public class NitroStudioInstrument : IOFile
    {
        public Instrument Inst;

        public override void Read(FileReader r)
        {
            _ = r.ReadUInt32();
            byte type = r.ReadByte();
            switch ((InstrumentType)type)
            {
                case InstrumentType.Blank:
                    break;
                case InstrumentType.DrumSet:
                    Inst = new DrumSetInstrument();
                    break;
                case InstrumentType.KeySplit:
                    Inst = new KeySplitInstrument();
                    break;
                default:
                    Inst = new DirectInstrument();
                    break;
            }
            if (r.ReadBoolean())
            {
                // Reading an empty instrument leaves Inst null; the caller reports it, because
                // this type must stay free of any UI framework.
                Inst = null;
                return;
            }
            Inst.Read(r);
            if ((Inst as DirectInstrument) != null)
            {
                Inst.NoteInfo[0].InstrumentType = (InstrumentType)type;
            }
        }

        public override void Write(FileWriter w)
        {
            w.Write("NIST".ToCharArray());
            w.Write((byte)(Inst == null ? 0 : Inst.Type()));
            w.Write(Inst == null);
            if (Inst != null)
            {
                w.Write(Inst);
            }
        }
    }
}
