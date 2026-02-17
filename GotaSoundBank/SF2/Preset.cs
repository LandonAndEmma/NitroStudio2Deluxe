using GotaSoundIO.IO;
using System.Collections.Generic;

namespace GotaSoundBank.SF2
{
    public class Preset : IReadable, IWriteable
    {
        public string Name = "";
        public ushort PresetNumber;
        public ushort Bank;
        public ushort ReadingBagIndex;
        public uint Library;
        public uint Genre;
        public uint Morphology;
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
            PresetNumber = r.ReadUInt16();
            Bank = r.ReadUInt16();
            ReadingBagIndex = r.ReadUInt16();
            Library = r.ReadUInt32();
            Genre = r.ReadUInt32();
            Morphology = r.ReadUInt32();
        }

        public void Write(FileWriter w)
        {
            w.WriteFixedString(Name, 20);
            w.Write(PresetNumber);
            w.Write(Bank);
            w.Write(ReadingBagIndex);
            w.Write(Library);
            w.Write(Genre);
            w.Write(Morphology);
        }
    }
}
