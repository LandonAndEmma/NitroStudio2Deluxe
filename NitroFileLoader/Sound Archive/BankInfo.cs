using GotaSequenceLib;
using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using DrumSetInstrument = NitroFileLoader.Instrument.DrumSetInstrument;
using InstrumentClass = NitroFileLoader.Instrument.Instrument;
using NitroInstrumentType = NitroFileLoader.Instrument.InstrumentType;
using NoteInfo = NitroFileLoader.Instrument.NoteInfo;

namespace NitroFileLoader
{
    public class BankInfo : IReadable, IWriteable
    {
        public string Name;
        public int Index;
        public bool ForceIndividualFile;
        public Bank File;
        public WaveArchiveInfo[] WaveArchives = { null, null, null, null };
        public uint ReadingFileId;
        public ushort ReadingWave0Id = 0xFFFF;
        public ushort ReadingWave1Id = 0xFFFF;
        public ushort ReadingWave2Id = 0xFFFF;
        public ushort ReadingWave3Id = 0xFFFF;

        public void Read(FileReader r)
        {
            ReadingFileId = r.ReadUInt32();
            ReadingWave0Id = r.ReadUInt16();
            ReadingWave1Id = r.ReadUInt16();
            ReadingWave2Id = r.ReadUInt16();
            ReadingWave3Id = r.ReadUInt16();
        }

        public void Write(FileWriter w)
        {
            w.Write(ReadingFileId);
            w.Write((ushort)(WaveArchives[0] == null ? ReadingWave0Id : WaveArchives[0].Index));
            w.Write((ushort)(WaveArchives[1] == null ? ReadingWave1Id : WaveArchives[1].Index));
            w.Write((ushort)(WaveArchives[2] == null ? ReadingWave2Id : WaveArchives[2].Index));
            w.Write((ushort)(WaveArchives[3] == null ? ReadingWave3Id : WaveArchives[3].Index));
        }

        public RiffWave[][] GetAssociatedWaves()
        {
            RiffWave[][] waves = new RiffWave[4][];
            for (int i = 0; i < 4; i++)
            {
                if (WaveArchives[i] != null)
                {
                    waves[i] = WaveArchives[i].File.GetWaves();
                }
            }
            return waves;
        }

        public void WriteTextFormat(string path, string name)
        {
            List<string> ret = ["@PATH \"../WaveArchives\"\n", "@INSTLIST"];
            int lastGroup = -1;
            int keyNum = 0;
            int drumNum = 0;
            foreach (InstrumentClass e in File.Instruments.OrderBy(x => x.GetOrder))
            {
                switch (e.Type())
                {
                    case NitroInstrumentType.DrumSet:
                        ret.Add("\t" + e.Index + " : DRUM_SET, _DRUM" + drumNum.ToString("D3"));
                        drumNum++;
                        break;
                    case NitroInstrumentType.KeySplit:
                        ret.Add("\t" + e.Index + " : KEY_SPLIT, _KEY" + keyNum.ToString("D3"));
                        keyNum++;
                        break;
                    default:
                        ret.Add(WriteNoteInfo(e.NoteInfo[0], e.Index.ToString()));
                        break;
                }
            }
            drumNum = 0;
            if (File.Instruments.Where(x => x.Type() == NitroInstrumentType.DrumSet).Count() > 0)
            {
                ret.Add("\n@DRUM_SET");
            }
            foreach (
                InstrumentClass e in File
                    .Instruments.OrderBy(x => x.GetOrder)
                    .Where(x => x.Type() == NitroInstrumentType.DrumSet)
            )
            {
                int regNum = 0;
                ret.Add("\n_DRUM" + drumNum.ToString("D3") + " =");
                Notes lastNote = 0;
                foreach (NoteInfo n in e.NoteInfo)
                {
                    Notes note = (Notes)(e as DrumSetInstrument).Min;
                    if (regNum != 0)
                    {
                        note = e.NoteInfo[regNum - 1].Key + 1;
                    }
                    lastNote = note;
                    ret.Add(WriteNoteInfo(n, note.ToString()));
                    regNum++;
                }
                if (lastNote != e.NoteInfo.Last().Key)
                {
                    ret.Add(WriteNoteInfo(e.NoteInfo.Last(), e.NoteInfo.Last().Key.ToString()));
                }
                drumNum++;
            }
            keyNum = 0;
            if (File.Instruments.Where(x => x.Type() == NitroInstrumentType.KeySplit).Count() > 0)
            {
                ret.Add("\n@KEY_SPLIT");
            }
            foreach (
                InstrumentClass e in File
                    .Instruments.OrderBy(x => x.GetOrder)
                    .Where(x => x.Type() == NitroInstrumentType.KeySplit)
            )
            {
                ret.Add("\n_KEY" + keyNum.ToString("D3") + " =");
                foreach (NoteInfo n in e.NoteInfo)
                {
                    ret.Add(WriteNoteInfo(n, n.Key.ToString()));
                }
                keyNum++;
            }
            string WriteNoteInfo(NoteInfo n, string ind)
            {
                switch (n.InstrumentType)
                {
                    case NitroInstrumentType.PSG:
                        return "\t"
                            + ind
                            + " : PSG, DUTY_"
                            + (n.WaveId + 1)
                            + "_8, "
                            + (Notes)n.BaseNote
                            + ", "
                            + n.Attack
                            + ", "
                            + n.Decay
                            + ", "
                            + n.Sustain
                            + ", "
                            + n.Release
                            + ", "
                            + n.Pan;
                    case NitroInstrumentType.Noise:
                        return "\t"
                            + ind
                            + " : NOISE, "
                            + (Notes)n.BaseNote
                            + ", "
                            + n.Attack
                            + ", "
                            + n.Decay
                            + ", "
                            + n.Sustain
                            + ", "
                            + n.Release
                            + ", "
                            + n.Pan;
                    case NitroInstrumentType.Null:
                        return "\t" + ind + " : NULL";
                    default:
                        if (WaveArchives[n.WarId] != null)
                        {
                            if (lastGroup != n.WarId)
                            {
                                ret.Add("@WGROUP " + n.WarId);
                                lastGroup = n.WarId;
                            }
                            return "\t"
                                + ind
                                + " : SWAV, \""
                                + WaveArchives[n.WarId].Name
                                + "/"
                                + n.WaveId.ToString("D4")
                                + ".adpcm.swav"
                                + "\", "
                                + (Notes)n.BaseNote
                                + ", "
                                + n.Attack
                                + ", "
                                + n.Decay
                                + ", "
                                + n.Sustain
                                + ", "
                                + n.Release
                                + ", "
                                + n.Pan;
                        }
                        break;
                }
                return "";
            }
            System.IO.File.WriteAllLines(path + "/" + name + ".bnk", ret);
        }
    }
}
