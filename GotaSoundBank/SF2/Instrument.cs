using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundBank.SF2 {
    public class Instrument : IReadable, IWriteable {
        public string Name = "";
        public ushort ReadingBagIndex;
        public int NumZones => Zones.Count + (GlobalZone != null ? 1 : 0);
        public Zone GlobalZone = null;
        public List<Zone> Zones = new List<Zone>();
        public List<Zone> GetAllZones() {
            List<Zone> ret = new List<Zone>();
            if (GlobalZone != null) { ret.Add(GlobalZone); }
            foreach (var z in Zones) {
                ret.Add(z);
            }
            return ret;
        }
        public void Read(FileReader r) {
            Name = r.ReadFixedString(20);
            ReadingBagIndex = r.ReadUInt16();
        }
        public void Write(FileWriter w) {
            w.WriteFixedString(Name, 20);
            w.Write(ReadingBagIndex);
        }
    }
}
