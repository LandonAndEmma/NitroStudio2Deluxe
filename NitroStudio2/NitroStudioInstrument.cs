using GotaSoundIO.IO;
using NitroFileLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace NitroStudio2 {
    public class NitroStudioInstrument : IOFile {
        public Instrument Inst;
        public override void Read(FileReader r) {
            r.ReadUInt32();
            byte type = r.ReadByte();
            switch ((InstrumentType)type) {
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
            if (r.ReadBoolean()) {
                MessageBox.Show("An empty instrument cannot be used!");
                return;
            }
            Inst.Read(r);
            if (Inst as DirectInstrument != null) {
                Inst.NoteInfo[0].InstrumentType = (InstrumentType)type;
            }
        }
        public override void Write(FileWriter w) {
            w.Write("NIST".ToCharArray());
            w.Write((byte)(Inst == null ? 0 : Inst.Type()));
            w.Write(Inst == null);
            if (Inst != null) { w.Write(Inst); }
        }
    }
}
