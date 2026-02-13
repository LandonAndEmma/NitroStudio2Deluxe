using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroFileLoader {
    public class WaveArchiveInfo : IReadable, IWriteable {
        public string Name;
        public int Index;
        public bool ForceIndividualFile;
        public WaveArchive File;
        public uint ReadingFileId;
        public bool LoadIndividually;
        public void Read(FileReader r) {
            ReadingFileId = r.ReadUInt32();
            LoadIndividually = (ReadingFileId & 0xFF000000) > 0;
            ReadingFileId &= 0xFFFFFF;
        }
        public void Write(FileWriter w) {
            w.Write((uint)((uint)ReadingFileId | (LoadIndividually ? 0x01000000U : 0)));
        }
        public void WriteTextFormat(string path, string name) {
            List<string> swls = new List<string>();
            int ind = 0;
            Directory.CreateDirectory(path + "/" + name);
            foreach (var w in File.Waves) {
                swls.Add(name + "/" + ind.ToString("D4") + ".adpcm.swav");
                w.Write(path + "/" + name + "/" + ind.ToString("D4") + ".adpcm.swav");
                RiffWave r = new RiffWave();
                r.FromOtherStreamFile(w);
                r.Write(path + "/" + name + "/" + ind.ToString("D4") + ".wav");
                ind++;
            }
            System.IO.File.WriteAllLines(path + "/" + name + ".swls", swls);
        }
    }
}
