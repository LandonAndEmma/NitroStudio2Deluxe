using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using System.Collections.Generic;
using System.IO;

namespace NitroFileLoader
{
    public class WaveArchiveInfo : IReadable, IWriteable
    {
        public string Name;
        public int Index;
        public bool ForceIndividualFile;
        public WaveArchive File;
        public uint ReadingFileId;
        public bool LoadIndividually;

        public void Read(FileReader r)
        {
            ReadingFileId = r.ReadUInt32();
            LoadIndividually = (ReadingFileId & 0xFF000000) > 0;
            ReadingFileId &= 0xFFFFFF;
        }

        public void Write(FileWriter w)
        {
            w.Write(ReadingFileId | (LoadIndividually ? 0x01000000U : 0));
        }

        public void WriteTextFormat(string path, string name)
        {
            List<string> swls = [];
            int ind = 0;
            _ = Directory.CreateDirectory(path + "/" + name);
            foreach (Wave w in File.Waves)
            {
                swls.Add(name + "/" + ind.ToString("D4") + ".adpcm.swav");
                w.Write(path + "/" + name + "/" + ind.ToString("D4") + ".adpcm.swav");
                RiffWave r = new();
                r.FromOtherStreamFile(w);
                r.Write(path + "/" + name + "/" + ind.ToString("D4") + ".wav");
                ind++;
            }
            System.IO.File.WriteAllLines(path + "/" + name + ".swls", swls);
        }
    }
}
