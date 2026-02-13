using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSequenceLib {
    public abstract class SequenceFile : IOFile {
        public List<SequenceCommand> Commands = new List<SequenceCommand>();
        public Dictionary<string, uint> Labels = new Dictionary<string, uint>();
        private Dictionary<uint, int> CommandIndices;
        public string Name;
        public byte[] RawData = new byte[0];
        public bool WritingCommandSuccess { get; protected set; } = true;
        public abstract SequencePlatform Platform();
        public Dictionary<string, int> PublicLabels { get; protected set; } = new Dictionary<string, int>();
        public List<int> OtherLabels { get; protected set; } = new List<int>();
        public SequenceFile() {}
        public SequenceFile(string filePath) : base(filePath) {}
        public void ReadCommandData(bool globalMode = false) {
            using (MemoryStream src = new MemoryStream(RawData)) {
                using (FileReader r = new FileReader(src)) {
                    int commandInd = 0;
                    var p = Platform();
                    Dictionary<uint, int> offsetMap = new Dictionary<uint, int>();
                    Commands = new List<SequenceCommand>();
                    PublicLabels = new Dictionary<string, int>();
                    OtherLabels = new List<int>();
                    while (r.Position < RawData.Length) {
                        offsetMap.Add((uint)r.Position, commandInd);
                        if (r.Position < RawData.Length - 1 && Commands.Count > 0 && Commands.Last().CommandType == SequenceCommands.Jump) {
                            long bak = r.Position;
                            if (r.ReadByte() == 0) {
                                continue;
                            } else {
                                r.Position = bak;
                            }
                        }
                        SequenceCommand c = new SequenceCommand();
                        c.Read(r, p);
                        Commands.Add(c);
                        commandInd++;
                    }
                    for (int i = 0; i < Labels.Count; i++) {
                        PublicLabels.Add(Labels.Keys.ElementAt(i), offsetMap[Labels.Values.ElementAt(i)]);
                    }
                    for (int i = 0; i < Commands.Count; i++) {
                        int commandIndex = 0;
                        SequenceCommands trueType = Playback.Player.GetTrueCommandType(Commands[i]);
                        switch (trueType) {
                            case SequenceCommands.Call:
                            case SequenceCommands.Jump:
                            case SequenceCommands.OpenTrack:
                                commandIndex = SetOffsetIndex(Commands[i], offsetMap);
                                break;
                        }
                        string label = "";
                        if (trueType == SequenceCommands.Call || trueType == SequenceCommands.Jump || trueType == SequenceCommands.OpenTrack) {
                            uint offset = offsetMap.FirstOrDefault(x => x.Value == commandIndex).Key;
                            if (Labels.ContainsValue(offset)) {
                                label = Labels.FirstOrDefault(x => x.Value == offset).Key;
                            } else {
                                label = (globalMode ? "C" : "_c") + "ommand_" + commandIndex;
                                OtherLabels.Add(commandIndex);
                            }
                        }
                        switch (trueType) {
                            case SequenceCommands.Call:
                            case SequenceCommands.Jump:
                            case SequenceCommands.OpenTrack:
                                SetCommandLabel(Commands[i], label);
                                break;
                        }
                    }
                    for (int i = 0; i < Commands.Count; i++) {
                        SequenceCommands trueType = Playback.Player.GetTrueCommandType(Commands[i]);
                        switch (trueType) {
                            case SequenceCommands.Call:
                            case SequenceCommands.Jump:
                            case SequenceCommands.OpenTrack:
                                SetReferenceCommand(Commands[i]);
                                break;
                        }
                    }
                    CommandIndices = offsetMap;
                }
            }
        }
        public void WriteCommandData() {
            using (MemoryStream o = new MemoryStream()) {
                using (FileWriter w = new FileWriter(o)) {
                    var p = Platform();
                    Dictionary<int, uint> indexMap = new Dictionary<int, uint>();
                    int commandInd = 0;
                    foreach (var c in Commands) {
                        indexMap.Add(commandInd, (uint)w.Position);
                        if (c.CommandType == SequenceCommands.Note || p.CommandMap().ContainsKey(c.CommandType) || p.ExtendedCommands().ContainsKey(c.CommandType)) {
                            c.Write(w, p);
                        }
                        commandInd++;
                    }
                    w.Position = 0;
                    Labels = new Dictionary<string, uint>();
                    for (int i = 0; i < PublicLabels.Count; i++) {
                        Labels.Add(PublicLabels.Keys.ElementAt(i), indexMap[PublicLabels.Values.ElementAt(i)]);
                    }
                    for (int i = 0; i < Commands.Count; i++) {
                        SequenceCommands trueCommandType = Playback.Player.GetTrueCommandType(Commands[i]);
                        switch (trueCommandType) {
                            case SequenceCommands.Call:
                            case SequenceCommands.Jump:
                            case SequenceCommands.OpenTrack:
                                SetIndexOffset(Commands[i], indexMap);
                                break;
                        }
                    }
                    foreach (var c in Commands) {
                        if (c.CommandType == SequenceCommands.Note || p.CommandMap().ContainsKey(c.CommandType) || p.ExtendedCommands().ContainsKey(c.CommandType)) {
                            c.Write(w, p);
                        }
                    }
                    RawData = o.ToArray();
                    CommandIndices = indexMap.ToDictionary(x => x.Value, x => x.Key);
                }
            }
        }
        public int SetOffsetIndex(SequenceCommand c, Dictionary<uint, int> offsetMap) {
            switch (c.CommandType) {
                case SequenceCommands.Random:
                case SequenceCommands.TimeRandom:
                    return SetOffsetIndex((c.Parameter as RandomParameter).Command, offsetMap);
                case SequenceCommands.If:
                    return SetOffsetIndex(c.Parameter as SequenceCommand, offsetMap);
                case SequenceCommands.Variable:
                case SequenceCommands.TimeVariable:
                    return SetOffsetIndex((c.Parameter as VariableParameter).Command, offsetMap);
                case SequenceCommands.Time:
                    return SetOffsetIndex((c.Parameter as TimeParameter).Command, offsetMap);
                case SequenceCommands.Jump:
                case SequenceCommands.Call:
                    (c.Parameter as UInt24Parameter).m_Index = offsetMap[(c.Parameter as UInt24Parameter).Offset];
                    return (c.Parameter as UInt24Parameter).m_Index;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).m_Index = offsetMap[(c.Parameter as OpenTrackParameter).Offset];
                    return (c.Parameter as OpenTrackParameter).m_Index;
            }
            return -1;
        }
        public int SetCommandLabel(SequenceCommand c, string label) {
            switch (c.CommandType) {
                case SequenceCommands.Random:
                case SequenceCommands.TimeRandom:
                    SetCommandLabel((c.Parameter as RandomParameter).Command, label);
                    break;
                case SequenceCommands.If:
                    SetCommandLabel(c.Parameter as SequenceCommand, label);
                    break;
                case SequenceCommands.Variable:
                case SequenceCommands.TimeVariable:
                    SetCommandLabel((c.Parameter as VariableParameter).Command, label);
                    break;
                case SequenceCommands.Time:
                    SetCommandLabel((c.Parameter as TimeParameter).Command, label);
                    break;
                case SequenceCommands.Jump:
                case SequenceCommands.Call:
                    (c.Parameter as UInt24Parameter).Label = label;
                    break;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).Label = label;
                    break;
            }
            return -1;
        }
        public uint SetIndexOffset(SequenceCommand c, Dictionary<int, uint> offsetMap) {
            switch (c.CommandType) {
                case SequenceCommands.Random:
                case SequenceCommands.TimeRandom:
                    return SetIndexOffset((c.Parameter as RandomParameter).Command, offsetMap);
                case SequenceCommands.If:
                    return SetIndexOffset(c.Parameter as SequenceCommand, offsetMap);
                case SequenceCommands.Variable:
                case SequenceCommands.TimeVariable:
                    return SetIndexOffset((c.Parameter as VariableParameter).Command, offsetMap);
                case SequenceCommands.Time:
                    return SetIndexOffset((c.Parameter as TimeParameter).Command, offsetMap);
                case SequenceCommands.Jump:
                case SequenceCommands.Call:
                    (c.Parameter as UInt24Parameter).Offset = offsetMap[(c.Parameter as UInt24Parameter).m_Index];
                    return (c.Parameter as UInt24Parameter).Offset;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).Offset = offsetMap[(c.Parameter as OpenTrackParameter).m_Index];
                    return (c.Parameter as OpenTrackParameter).Offset;
            }
            return 0xFFFFFFFF;
        }
        public void SetReferenceCommand(SequenceCommand c) {
            switch (c.CommandType) {
                case SequenceCommands.Random:
                case SequenceCommands.TimeRandom:
                    SetReferenceCommand((c.Parameter as RandomParameter).Command);
                    break;
                case SequenceCommands.If:
                    SetReferenceCommand(c.Parameter as SequenceCommand);
                    break;
                case SequenceCommands.Variable:
                case SequenceCommands.TimeVariable:
                    SetReferenceCommand((c.Parameter as VariableParameter).Command);
                    break;
                case SequenceCommands.Time:
                    SetReferenceCommand((c.Parameter as TimeParameter).Command);
                    break;
                case SequenceCommands.Jump:
                case SequenceCommands.Call:
                    (c.Parameter as UInt24Parameter).ReferenceCommand = Commands[(c.Parameter as UInt24Parameter).Index(Commands)];
                    break;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).ReferenceCommand = Commands[(c.Parameter as OpenTrackParameter).Index(Commands)];
                    break;
            }
        }
        public string[] ToText() {
            ReadCommandData();
            List<string> l = new List<string>();
            l.Add(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            l.Add(";");
            l.Add("; " + Name);
            l.Add(";     Generated By Gota's Sound Tools");
            l.Add(";");
            l.Add(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            l.Add("");
            for (int i = 0; i < Commands.Count; i++) {
                bool labelAdded = false;
                var labels = PublicLabels.Where(x => x.Value == i).Select(x => x.Key);
                foreach (var label in labels) {
                    if (i != 0 && !labelAdded && Commands[i - 1].CommandType == SequenceCommands.Fin) {
                        l.Add(" ");
                    }
                    l.Add(label + ":");
                    labelAdded = true;
                }
                if (OtherLabels.Contains(i)) {
                    if (i != 0 && !labelAdded && Commands[i - 1].CommandType == SequenceCommands.Fin) {
                        l.Add(" ");
                    }
                    l.Add("_command_" + i + ":");
                    labelAdded = true;
                }
                if (i < Commands.Count - 1) {
                    l.Add("\t" + Commands[i].ToString());
                }
            }
            return l.ToArray();
        }
        public void FromText(List<string> text) {
            WritingCommandSuccess = true;
            PublicLabels = new Dictionary<string, int>();
            OtherLabels = new List<int>();
            Dictionary<string, int> privateLabels = new Dictionary<string, int>();
            List<int> labelLines = new List<int>();
            List<string> t = text.ToList();
            int comNum = 0;
            for (int i = t.Count - 1; i >= 0; i--) {
                t[i] = t[i].Replace("\t", " ").Replace("\r", "").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                try { t[i] = t[i].Split(';')[0]; } catch { }
                if (t[i].Replace(" ", "").Length == 0) { t.RemoveAt(i); continue; }
                for (int j = 0; j < t[i].Length; j++) {
                    if (t[i][j].Equals(' ')) {
                        t[i] = t[i].Substring(j + 1);
                        j--;
                    } else {
                        break;
                    }
                }
            }
            for (int i = 0; i < t.Count; i++) {
                if (t[i].EndsWith(":")) {
                    labelLines.Add(i);
                    if (t[i].StartsWith("_")) {
                        privateLabels.Add(t[i].Replace(":", ""), comNum);
                        OtherLabels.Add(comNum);
                    } else {
                        PublicLabels.Add(t[i].Replace(":", ""), comNum);
                    }
                } else {
                    comNum++;
                }
            }
            PublicLabels = PublicLabels.OrderBy(obj => new NullTerminatedString(obj.Key)).ToDictionary(obj => obj.Key, obj => obj.Value);
            Commands = new List<SequenceCommand>();
            for (int i = 0; i < t.Count; i++) {
                if (labelLines.Contains(i)) {
                    continue;
                }
                SequenceCommand seq = new SequenceCommand();
                try { seq.FromString(t[i], PublicLabels, privateLabels); } catch (Exception e) { WritingCommandSuccess = false; throw new Exception("Command " + i + ": \"" + t[i] + "\" is invalid.", e); }
                Commands.Add(seq);
            }
            for (int i = 0; i < Commands.Count; i++) {
                SequenceCommands trueType = Playback.Player.GetTrueCommandType(Commands[i]);
                switch (trueType) {
                    case SequenceCommands.Call:
                    case SequenceCommands.Jump:
                    case SequenceCommands.OpenTrack:
                        SetReferenceCommand(Commands[i]);
                        break;
                }
            }
            Commands.Add(new SequenceCommand() { CommandType = SequenceCommands.Fin });
            WriteCommandData();
        }
        public int ConvertOffset(uint offset) {
            int lowest = -1;
            long minDist = long.MaxValue;
            for (int i = 0; i < CommandIndices.Count; i++) {
                long dist = Math.Abs(offset - CommandIndices.Keys.ElementAt(i));
                if (dist < minDist) {
                    minDist = dist;
                    lowest = i;
                }
            }
            return CommandIndices.Values.ElementAt(lowest);
        }
        public void FromMIDI(string filePath, int timeBase = 48, bool privateLabelsForCalls = false) {
            Sanford.Multimedia.Midi.Sequence s = new Sanford.Multimedia.Midi.Sequence(filePath);
            Dictionary<string, int> pub;
            List<int> priv;
            Commands = SMF.ToSequenceCommands(s, out pub, out priv, Path.GetFileNameWithoutExtension(filePath), timeBase);
            PublicLabels = pub;
            OtherLabels = priv;
            WriteCommandData();
        }
        public void SaveMIDI(string filePath, ushort trackMask = 0xFFFF) {
            ReadCommandData();
            Sanford.Multimedia.Midi.Sequence s = SMF.FromSequenceCommands(Commands, 0, trackMask);
            s.Save(filePath);
        }
        public void CopyFromOther(SequenceFile other) {
            other.ReadCommandData();
            Commands = other.Commands;
            PublicLabels = other.PublicLabels;
            OtherLabels = other.OtherLabels;
        }
    }
}
