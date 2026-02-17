using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GotaSequenceLib
{
    public abstract class SequenceFile : IOFile
    {
        public List<SequenceCommand> Commands = [];
        public Dictionary<string, uint> Labels = [];
        private Dictionary<uint, int> CommandIndices;
        public string Name;
        public byte[] RawData = new byte[0];
        public bool WritingCommandSuccess { get; protected set; } = true;
        public abstract SequencePlatform Platform();
        public Dictionary<string, int> PublicLabels { get; protected set; } =
            [];
        public List<int> OtherLabels { get; protected set; } = [];

        public SequenceFile() { }

        public SequenceFile(string filePath)
            : base(filePath) { }

        public void ReadCommandData(bool globalMode = false)
        {
            using MemoryStream src = new(RawData);
            using FileReader r = new(src);
            int commandInd = 0;
            SequencePlatform p = Platform();
            Dictionary<uint, int> offsetMap = [];
            Commands = [];
            PublicLabels = [];
            OtherLabels = [];
            while (r.Position < RawData.Length)
            {
                offsetMap.Add((uint)r.Position, commandInd);
                if (
                    r.Position < RawData.Length - 1
                    && Commands.Count > 0
                    && Commands.Last().CommandType == SequenceCommands.Jump
                )
                {
                    long bak = r.Position;
                    if (r.ReadByte() == 0)
                    {
                        continue;
                    }
                    else
                    {
                        r.Position = bak;
                    }
                }
                SequenceCommand c = new();
                c.Read(r, p);
                Commands.Add(c);
                commandInd++;
            }
            for (int i = 0; i < Labels.Count; i++)
            {
                PublicLabels.Add(
                    Labels.Keys.ElementAt(i),
                    offsetMap[Labels.Values.ElementAt(i)]
                );
            }
            for (int i = 0; i < Commands.Count; i++)
            {
                int commandIndex = 0;
                SequenceCommands trueType = Playback.Player.GetTrueCommandType(Commands[i]);
                switch (trueType)
                {
                    case SequenceCommands.Call:
                    case SequenceCommands.Jump:
                    case SequenceCommands.OpenTrack:
                        commandIndex = SetOffsetIndex(Commands[i], offsetMap);
                        break;
                }
                string label = "";
                if (
                    trueType is SequenceCommands.Call
                    or SequenceCommands.Jump
                    or SequenceCommands.OpenTrack
                )
                {
                    uint offset = offsetMap
                        .FirstOrDefault(x => x.Value == commandIndex)
                        .Key;
                    if (Labels.ContainsValue(offset))
                    {
                        label = Labels.FirstOrDefault(x => x.Value == offset).Key;
                    }
                    else
                    {
                        label = (globalMode ? "C" : "_c") + "ommand_" + commandIndex;
                        OtherLabels.Add(commandIndex);
                    }
                }
                switch (trueType)
                {
                    case SequenceCommands.Call:
                    case SequenceCommands.Jump:
                    case SequenceCommands.OpenTrack:
                        _ = SetCommandLabel(Commands[i], label);
                        break;
                }
            }
            for (int i = 0; i < Commands.Count; i++)
            {
                SequenceCommands trueType = Playback.Player.GetTrueCommandType(Commands[i]);
                switch (trueType)
                {
                    case SequenceCommands.Call:
                    case SequenceCommands.Jump:
                    case SequenceCommands.OpenTrack:
                        SetReferenceCommand(Commands[i]);
                        break;
                }
            }
            CommandIndices = offsetMap;
        }

        public void WriteCommandData()
        {
            using MemoryStream o = new();
            using FileWriter w = new(o);
            SequencePlatform p = Platform();
            Dictionary<int, uint> indexMap = [];
            int commandInd = 0;
            foreach (SequenceCommand c in Commands)
            {
                indexMap.Add(commandInd, (uint)w.Position);
                if (
                    c.CommandType == SequenceCommands.Note
                    || p.CommandMap().ContainsKey(c.CommandType)
                    || p.ExtendedCommands().ContainsKey(c.CommandType)
                )
                {
                    c.Write(w, p);
                }
                commandInd++;
            }
            w.Position = 0;
            Labels = [];
            for (int i = 0; i < PublicLabels.Count; i++)
            {
                Labels.Add(
                    PublicLabels.Keys.ElementAt(i),
                    indexMap[PublicLabels.Values.ElementAt(i)]
                );
            }
            for (int i = 0; i < Commands.Count; i++)
            {
                SequenceCommands trueCommandType = Playback.Player.GetTrueCommandType(
                    Commands[i]
                );
                switch (trueCommandType)
                {
                    case SequenceCommands.Call:
                    case SequenceCommands.Jump:
                    case SequenceCommands.OpenTrack:
                        _ = SetIndexOffset(Commands[i], indexMap);
                        break;
                }
            }
            foreach (SequenceCommand c in Commands)
            {
                if (
                    c.CommandType == SequenceCommands.Note
                    || p.CommandMap().ContainsKey(c.CommandType)
                    || p.ExtendedCommands().ContainsKey(c.CommandType)
                )
                {
                    c.Write(w, p);
                }
            }
            RawData = o.ToArray();
            CommandIndices = indexMap.ToDictionary(x => x.Value, x => x.Key);
        }

        public int SetOffsetIndex(SequenceCommand c, Dictionary<uint, int> offsetMap)
        {
            switch (c.CommandType)
            {
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
                    (c.Parameter as UInt24Parameter).m_Index = offsetMap[
                        (c.Parameter as UInt24Parameter).Offset
                    ];
                    return (c.Parameter as UInt24Parameter).m_Index;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).m_Index = offsetMap[
                        (c.Parameter as OpenTrackParameter).Offset
                    ];
                    return (c.Parameter as OpenTrackParameter).m_Index;
            }
            return -1;
        }

        public int SetCommandLabel(SequenceCommand c, string label)
        {
            switch (c.CommandType)
            {
                case SequenceCommands.Random:
                case SequenceCommands.TimeRandom:
                    _ = SetCommandLabel((c.Parameter as RandomParameter).Command, label);
                    break;
                case SequenceCommands.If:
                    _ = SetCommandLabel(c.Parameter as SequenceCommand, label);
                    break;
                case SequenceCommands.Variable:
                case SequenceCommands.TimeVariable:
                    _ = SetCommandLabel((c.Parameter as VariableParameter).Command, label);
                    break;
                case SequenceCommands.Time:
                    _ = SetCommandLabel((c.Parameter as TimeParameter).Command, label);
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

        public uint SetIndexOffset(SequenceCommand c, Dictionary<int, uint> offsetMap)
        {
            switch (c.CommandType)
            {
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
                    (c.Parameter as UInt24Parameter).Offset = offsetMap[
                        (c.Parameter as UInt24Parameter).m_Index
                    ];
                    return (c.Parameter as UInt24Parameter).Offset;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).Offset = offsetMap[
                        (c.Parameter as OpenTrackParameter).m_Index
                    ];
                    return (c.Parameter as OpenTrackParameter).Offset;
            }
            return 0xFFFFFFFF;
        }

        public void SetReferenceCommand(SequenceCommand c)
        {
            switch (c.CommandType)
            {
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
                    (c.Parameter as UInt24Parameter).ReferenceCommand = Commands[
                        (c.Parameter as UInt24Parameter).Index(Commands)
                    ];
                    break;
                case SequenceCommands.OpenTrack:
                    (c.Parameter as OpenTrackParameter).ReferenceCommand = Commands[
                        (c.Parameter as OpenTrackParameter).Index(Commands)
                    ];
                    break;
            }
        }

        public string[] ToText()
        {
            ReadCommandData();
            List<string> l =
            [
                ";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
                ";",
                "; " + Name,
                ";     Generated By Gota's Sound Tools",
                ";",
                ";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
                "",
            ];
            for (int i = 0; i < Commands.Count; i++)
            {
                bool labelAdded = false;
                IEnumerable<string> labels = PublicLabels.Where(x => x.Value == i).Select(x => x.Key);
                foreach (string label in labels)
                {
                    if (
                        i != 0
                        && !labelAdded
                        && Commands[i - 1].CommandType == SequenceCommands.Fin
                    )
                    {
                        l.Add(" ");
                    }
                    l.Add(label + ":");
                    labelAdded = true;
                }
                if (OtherLabels.Contains(i))
                {
                    if (
                        i != 0
                        && !labelAdded
                        && Commands[i - 1].CommandType == SequenceCommands.Fin
                    )
                    {
                        l.Add(" ");
                    }
                    l.Add("_command_" + i + ":");
                    labelAdded = true;
                }
                if (i < Commands.Count - 1)
                {
                    l.Add("\t" + Commands[i].ToString());
                }
            }
            return l.ToArray();
        }

        public void FromText(List<string> text)
        {
            WritingCommandSuccess = true;
            PublicLabels = [];
            OtherLabels = [];
            Dictionary<string, int> privateLabels = [];
            List<int> labelLines = [];
            List<string> t = text.ToList();
            int comNum = 0;
            for (int i = t.Count - 1; i >= 0; i--)
            {
                t[i] = t[i]
                    .Replace("\t", " ")
                    .Replace("\r", "")
                    .Replace("  ", " ")
                    .Replace("  ", " ")
                    .Replace("  ", " ")
                    .Replace("  ", " ")
                    .Replace("  ", " ");
                try
                {
                    t[i] = t[i].Split(';')[0];
                }
                catch { }
                if (t[i].Replace(" ", "").Length == 0)
                {
                    t.RemoveAt(i);
                    continue;
                }
                for (int j = 0; j < t[i].Length; j++)
                {
                    if (t[i][j].Equals(' '))
                    {
                        t[i] = t[i][(j + 1)..];
                        j--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i].EndsWith(":"))
                {
                    labelLines.Add(i);
                    if (t[i].StartsWith("_"))
                    {
                        privateLabels.Add(t[i].Replace(":", ""), comNum);
                        OtherLabels.Add(comNum);
                    }
                    else
                    {
                        PublicLabels.Add(t[i].Replace(":", ""), comNum);
                    }
                }
                else
                {
                    comNum++;
                }
            }
            PublicLabels = PublicLabels
                .OrderBy(obj => new NullTerminatedString(obj.Key))
                .ToDictionary(obj => obj.Key, obj => obj.Value);
            Commands = [];
            for (int i = 0; i < t.Count; i++)
            {
                if (labelLines.Contains(i))
                {
                    continue;
                }
                SequenceCommand seq = new();
                try
                {
                    seq.FromString(t[i], PublicLabels, privateLabels);
                }
                catch (Exception e)
                {
                    WritingCommandSuccess = false;
                    throw new Exception("Command " + i + ": \"" + t[i] + "\" is invalid.", e);
                }
                Commands.Add(seq);
            }
            for (int i = 0; i < Commands.Count; i++)
            {
                SequenceCommands trueType = Playback.Player.GetTrueCommandType(Commands[i]);
                switch (trueType)
                {
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

        public int ConvertOffset(uint offset)
        {
            int lowest = -1;
            long minDist = long.MaxValue;
            for (int i = 0; i < CommandIndices.Count; i++)
            {
                long dist = Math.Abs(offset - CommandIndices.Keys.ElementAt(i));
                if (dist < minDist)
                {
                    minDist = dist;
                    lowest = i;
                }
            }
            return CommandIndices.Values.ElementAt(lowest);
        }

        public void FromMIDI(string filePath, int timeBase = 48, bool privateLabelsForCalls = false)
        {
            Sanford.Multimedia.Midi.Sequence s = new(filePath);
            Commands = SMF.ToSequenceCommands(
                s,
                out Dictionary<string, int> pub,
                out List<int> priv,
                Path.GetFileNameWithoutExtension(filePath),
                timeBase
            );
            PublicLabels = pub;
            OtherLabels = priv;
            WriteCommandData();
        }

        public void SaveMIDI(string filePath, ushort trackMask = 0xFFFF)
        {
            ReadCommandData();
            Sanford.Multimedia.Midi.Sequence s = SMF.FromSequenceCommands(Commands, 0, trackMask);
            s.Save(filePath);
        }

        public void CopyFromOther(SequenceFile other)
        {
            other.ReadCommandData();
            Commands = other.Commands;
            PublicLabels = other.PublicLabels;
            OtherLabels = other.OtherLabels;
        }
    }
}
