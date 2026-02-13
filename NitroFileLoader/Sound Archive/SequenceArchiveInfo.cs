using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class SequenceArchiveInfo : IReadable, IWriteable {
        public string Name;
        public int Index;
        public bool ForceIndividualFile;
        public SequenceArchive File;
        public uint ReadingFileId;
        public void Read(FileReader r) {
            ReadingFileId = r.ReadUInt32();
        }
        public void Write(FileWriter w) {
            w.Write(ReadingFileId);
        }
    }
}
