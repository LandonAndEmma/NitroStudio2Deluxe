using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class GroupInfo : IReadable, IWriteable {
        public string Name;
        public int Index;
        public List<GroupEntry> Entries = new List<GroupEntry>();
        public void Read(FileReader r) {
            Entries = new List<GroupEntry>();
            uint numEntries = r.ReadUInt32();
            for (uint i = 0; i < numEntries; i++) {
                Entries.Add(r.Read<GroupEntry>());
            }
        }
        public void Write(FileWriter w) {
            Entries = Entries.Where(x => x.Entry != null).ToList();
            w.Write((uint)Entries.Count);
            foreach (var e in Entries) {
                w.Write(e);
            }
        }
    }
}
