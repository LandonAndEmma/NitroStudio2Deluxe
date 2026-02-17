using GotaSoundIO.IO;
using System.Collections.Generic;

namespace GotaSoundBank.SF2
{
    public class Instrument : IReadable, IWriteable
    {
        public string Name = "";
        public ushort ReadingBagIndex;
        public int NumZones => Zones.Count + (GlobalZone != null ? 1 : 0);
        public Zone GlobalZone = null;
        public List<Zone> Zones = [];

        public List<Zone> GetAllZones()
        {
            List<Zone> ret = [];
            if (GlobalZone != null)
            {
                ret.Add(GlobalZone);
            }
            foreach (Zone z in Zones)
            {
                ret.Add(z);
            }
            return ret;
        }

        public void Read(FileReader r)
        {
            Name = r.ReadFixedString(20);
            ReadingBagIndex = r.ReadUInt16();
        }

        public void Write(FileWriter w)
        {
            w.WriteFixedString(Name, 20);
            w.Write(ReadingBagIndex);
        }
    }
}
