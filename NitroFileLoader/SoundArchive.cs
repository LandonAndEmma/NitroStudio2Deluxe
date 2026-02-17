using GotaSoundIO;
using GotaSoundIO.IO;
using GotaSoundIO.Sound.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NitroFileLoader
{
    public class SoundArchive : IOFile
    {
        public const uint MaxSequenceId = 0xFFFFFFFF;
        public const uint MaxSequenceArchiveId = 0xFFFFFFFF;
        public const uint MaxBankId = 0xFFFF;
        public const uint MaxWaveArchiveId = 0xFFFE;
        public const uint MaxPlayerId = 31;
        public const uint MaxGroupId = 0xFFFFFFFF;
        public const uint MaxStreamPlayerId = 3;
        public const uint MaxStreamId = 0xFFFFFFFF;
        public List<SequenceInfo> Sequences = [];
        public List<SequenceArchiveInfo> SequenceArchives = [];
        public List<BankInfo> Banks = [];
        public List<WaveArchiveInfo> WaveArchives = [];
        public List<PlayerInfo> Players = [];
        public List<GroupInfo> Groups = [];
        public List<StreamPlayerInfo> StreamPlayers = [];
        public List<StreamInfo> Streams = [];
        public bool SaveSymbols = true;

        public SoundArchive() { }

        public SoundArchive(string filePath)
            : base(filePath) { }

        public override void Read(FileReader r)
        {
            r.OpenFile<SDATHeader>(out FileHeader header);
            List<string> seqNames = [];
            List<string> seqArcNames = [];
            List<List<string>> seqArcSequenceNames = [];
            List<string> bankNames = [];
            List<string> warNames = [];
            List<string> playerNames = [];
            List<string> groupNames = [];
            List<string> streamPlayerNames = [];
            List<string> streamNames = [];
            if (header.BlockOffsets.Length > 3)
            {
                SaveSymbols = true;
                r.OpenBlock(0, out _, out _, false);
                _ = r.ReadUInt64();
                r.OpenOffset("seqNames");
                r.OpenOffset("seqArcNames");
                r.OpenOffset("bankNames");
                r.OpenOffset("warNames");
                r.OpenOffset("playerNames");
                r.OpenOffset("groupNames");
                r.OpenOffset("streamPlayerNames");
                r.OpenOffset("streamNames");
                List<string> ReadNameData(string name)
                {
                    List<string> s = [];
                    r.JumpToOffset(name);
                    Table<uint> nameOffs = r.Read<Table<uint>>();
                    foreach (uint u in nameOffs)
                    {
                        if (u == 0)
                        {
                            s.Add(null);
                        }
                        else
                        {
                            r.Jump(u);
                            s.Add(r.ReadNullTerminated());
                        }
                    }
                    return s;
                }
                seqNames = ReadNameData("seqNames");
                bankNames = ReadNameData("bankNames");
                warNames = ReadNameData("warNames");
                playerNames = ReadNameData("playerNames");
                groupNames = ReadNameData("groupNames");
                streamPlayerNames = ReadNameData("streamPlayerNames");
                streamNames = ReadNameData("streamNames");
                r.JumpToOffset("seqArcNames");
                uint numSeqArcs = r.ReadUInt32();
                for (uint i = 0; i < numSeqArcs; i++)
                {
                    r.OpenOffset("seqArcName" + i);
                    r.OpenOffset("seqArcSequenceNames" + i);
                }
                for (uint i = 0; i < numSeqArcs; i++)
                {
                    if (!r.OffsetNull("seqArcName" + i))
                    {
                        r.JumpToOffset("seqArcName" + i);
                        seqArcNames.Add(r.ReadNullTerminated());
                    }
                    else
                    {
                        seqArcNames.Add(null);
                    }
                    if (!r.OffsetNull("seqArcSequenceNames" + i))
                    {
                        seqArcSequenceNames.Add(ReadNameData("seqArcSequenceNames" + i));
                    }
                    else
                    {
                        seqArcSequenceNames.Add(null);
                    }
                }
            }
            else
            {
                SaveSymbols = false;
            }
            r.OpenBlock(header.BlockOffsets.Length > 3 ? 2 : 1, out _, out _);
            uint numFiles = r.ReadUInt32();
            List<Tuple<uint, uint>> fileOffs = [];
            for (uint i = 0; i < numFiles; i++)
            {
                fileOffs.Add(new Tuple<uint, uint>(r.ReadUInt32(), r.ReadUInt32()));
                _ = r.ReadUInt64();
            }
            r.OpenBlock(header.BlockOffsets.Length > 3 ? 1 : 0, out _, out _, false);
            _ = r.ReadUInt64();
            Sequences = [];
            SequenceArchives = [];
            Banks = [];
            WaveArchives = [];
            Players = [];
            Groups = [];
            StreamPlayers = [];
            Streams = [];
            r.OpenOffset("seqInfo");
            r.OpenOffset("seqArcInfo");
            r.OpenOffset("bankInfo");
            r.OpenOffset("warInfo");
            r.OpenOffset("playerInfo");
            r.OpenOffset("groupInfo");
            r.OpenOffset("streamPlayerInfo");
            r.OpenOffset("streamInfo");
            Dictionary<string, List<uint>> md5Ids = [];
            r.JumpToOffset("playerInfo");
            Table<uint> offs = r.Read<Table<uint>>();
            int ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    Players.Add(r.Read<PlayerInfo>());
                    Players.Last().Index = ind;
                    Players.Last().Name =
                        ind > (playerNames.Count - 1) ? "PLAYER_" + ind : playerNames[ind];
                }
                ind++;
            }
            r.JumpToOffset("streamPlayerInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    StreamPlayers.Add(r.Read<StreamPlayerInfo>());
                    StreamPlayers.Last().Index = ind;
                    StreamPlayers.Last().Name =
                        ind > (streamPlayerNames.Count - 1)
                            ? "STRM_PLAYER_" + ind
                            : streamPlayerNames[ind];
                }
                ind++;
            }
            List<string> invalidWaveErrors = [];
            r.JumpToOffset("warInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    WaveArchives.Add(r.Read<WaveArchiveInfo>());
                    WaveArchives.Last().Index = ind;
                    WaveArchives.Last().Name =
                        ind > (warNames.Count - 1) ? "WAVE_ARCHIVE_" + ind : warNames[ind];
                    r.Jump(fileOffs[(int)WaveArchives.Last().ReadingFileId].Item1, true);
                    WaveArchives.Last().File = r.ReadFile<WaveArchive>();
                    try
                    {
                        string md5 = WaveArchives.Last().File.Md5Sum;
                        if (!md5Ids.ContainsKey(md5))
                        {
                            md5Ids.Add(md5, [WaveArchives.Last().ReadingFileId]);
                        }
                        else
                        {
                            if (!md5Ids[md5].Contains(WaveArchives.Last().ReadingFileId))
                            {
                                WaveArchives.Last().ForceIndividualFile = true;
                            }
                        }
                    }
                    catch (InvalidWaveException ex)
                    {
                        string archiveName = WaveArchives.Last().Name;
                        int waveIndex = -1;
                        if (
                            WaveArchives.Last().File != null
                            && WaveArchives.Last().File.Waves != null
                        )
                        {
                            for (int w = 0; w < WaveArchives.Last().File.Waves.Count; w++)
                            {
                                if (WaveArchives.Last().File.Waves[w].SampleRate == 0)
                                {
                                    waveIndex = w;
                                    break;
                                }
                            }
                        }
                        string errorMsg = $"Wave Archive '{archiveName}' (Index {ind})";
                        if (waveIndex >= 0)
                        {
                            errorMsg += $" - Wave {waveIndex}: {ex.Message}";
                        }
                        else
                        {
                            errorMsg += $": {ex.Message}";
                        }
                        invalidWaveErrors.Add(errorMsg);
                    }
                }
                ind++;
            }
            r.JumpToOffset("bankInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    Banks.Add(r.Read<BankInfo>());
                    Banks.Last().Index = ind;
                    Banks.Last().Name =
                        ind > (bankNames.Count - 1) ? "BANK_" + ind : bankNames[ind];
                    r.Jump(fileOffs[(int)Banks.Last().ReadingFileId].Item1, true);
                    Banks.Last().File = r.ReadFile<Bank>();
                    Banks.Last().WaveArchives[0] =
                        Banks.Last().ReadingWave0Id == 0xFFFF
                            ? null
                            : WaveArchives
                                .Where(x => x.Index == Banks.Last().ReadingWave0Id)
                                .FirstOrDefault();
                    Banks.Last().WaveArchives[1] =
                        Banks.Last().ReadingWave1Id == 0xFFFF
                            ? null
                            : WaveArchives
                                .Where(x => x.Index == Banks.Last().ReadingWave1Id)
                                .FirstOrDefault();
                    Banks.Last().WaveArchives[2] =
                        Banks.Last().ReadingWave2Id == 0xFFFF
                            ? null
                            : WaveArchives
                                .Where(x => x.Index == Banks.Last().ReadingWave2Id)
                                .FirstOrDefault();
                    Banks.Last().WaveArchives[3] =
                        Banks.Last().ReadingWave3Id == 0xFFFF
                            ? null
                            : WaveArchives
                                .Where(x => x.Index == Banks.Last().ReadingWave3Id)
                                .FirstOrDefault();
                    string md5 = Banks.Last().File.Md5Sum;
                    if (!md5Ids.ContainsKey(md5))
                    {
                        md5Ids.Add(md5, [Banks.Last().ReadingFileId]);
                    }
                    else
                    {
                        if (!md5Ids[md5].Contains(Banks.Last().ReadingFileId))
                        {
                            Banks.Last().ForceIndividualFile = true;
                        }
                    }
                }
                ind++;
            }
            r.JumpToOffset("seqInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    Sequences.Add(r.Read<SequenceInfo>());
                    Sequences.Last().Index = ind;
                    Sequences.Last().Name =
                        ind > (seqNames.Count - 1) ? "SEQ_" + ind : seqNames[ind];
                    r.Jump(fileOffs[(int)Sequences.Last().ReadingFileId].Item1, true);
                    Sequences.Last().File = r.ReadFile<Sequence>();
                    Sequences.Last().Bank = Banks
                        .Where(x => x.Index == Sequences.Last().ReadingBankId)
                        .FirstOrDefault();
                    Sequences.Last().Player = Players
                        .Where(x => x.Index == Sequences.Last().ReadingPlayerId)
                        .FirstOrDefault();
                    string md5 = Sequences.Last().File.Md5Sum;
                    if (!md5Ids.ContainsKey(md5))
                    {
                        md5Ids.Add(md5, [Sequences.Last().ReadingFileId]);
                    }
                    else
                    {
                        if (!md5Ids[md5].Contains(Sequences.Last().ReadingFileId))
                        {
                            Sequences.Last().ForceIndividualFile = true;
                        }
                    }
                }
                ind++;
            }
            r.JumpToOffset("streamInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    Streams.Add(r.Read<StreamInfo>());
                    Streams.Last().Index = ind;
                    Streams.Last().Name =
                        ind > (streamNames.Count - 1) ? "STRM_" + ind : streamNames[ind];
                    r.Jump(fileOffs[(int)Streams.Last().ReadingFileId].Item1, true);
                    Streams.Last().File = r.ReadFile<Stream>();
                    Streams.Last().Player = StreamPlayers
                        .Where(x => x.Index == Streams.Last().ReadingPlayerId)
                        .FirstOrDefault();
                    string md5 = Streams.Last().File.Md5Sum;
                    if (!md5Ids.ContainsKey(md5))
                    {
                        md5Ids.Add(md5, [Streams.Last().ReadingFileId]);
                    }
                    else
                    {
                        if (!md5Ids[md5].Contains(Streams.Last().ReadingFileId))
                        {
                            Streams.Last().ForceIndividualFile = true;
                        }
                    }
                }
                ind++;
            }
            r.JumpToOffset("seqArcInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    SequenceArchives.Add(r.Read<SequenceArchiveInfo>());
                    SequenceArchives.Last().Index = ind;
                    SequenceArchives.Last().Name =
                        ind > (seqArcNames.Count - 1) ? "SEQARC_" + ind : seqArcNames[ind];
                    r.Jump(fileOffs[(int)SequenceArchives.Last().ReadingFileId].Item1, true);
                    SequenceArchives.Last().File = r.ReadFile<SequenceArchive>();
                    Dictionary<string, uint> labels = SequenceArchives.Last().File.Labels;
                    SequenceArchives.Last().File.Labels = [];
                    if (SequenceArchives.Last().File.Sequences.Count > 0)
                    {
                        int seqNum = 0;
                        for (
                            int i = 0;
                            i <= SequenceArchives.Last().File.Sequences.Last().Index;
                            i++
                        )
                        {
                            string defName = "Sequence_" + i;
                            try
                            {
                                defName = seqArcSequenceNames[ind][i];
                            }
                            catch { }
                            SequenceArchiveSequence e = SequenceArchives
                                .Last()
                                .File.Sequences.Where(x => x.Index == i)
                                .FirstOrDefault();
                            if (defName != null && e != null)
                            {
                                e.Name = defName;
                                e.Bank = Banks
                                    .Where(x => x.Index == e.ReadingBankId)
                                    .FirstOrDefault();
                                e.Player = Players
                                    .Where(x => x.Index == e.ReadingPlayerId)
                                    .FirstOrDefault();
                                if (!SequenceArchives.Last().File.Labels.ContainsKey(defName))
                                {
                                    SequenceArchives
                                        .Last()
                                        .File.Labels.Add(defName, labels.Values.ElementAt(seqNum));
                                }
                                seqNum++;
                            }
                        }
                    }
                    string md5 = SequenceArchives.Last().File.Md5Sum;
                    if (!md5Ids.ContainsKey(md5))
                    {
                        md5Ids.Add(md5, [SequenceArchives.Last().ReadingFileId]);
                    }
                    else
                    {
                        if (!md5Ids[md5].Contains(SequenceArchives.Last().ReadingFileId))
                        {
                            SequenceArchives.Last().ForceIndividualFile = true;
                        }
                    }
                }
                ind++;
            }
            r.JumpToOffset("groupInfo");
            offs = r.Read<Table<uint>>();
            ind = 0;
            foreach (uint o in offs)
            {
                if (o != 0)
                {
                    r.Jump(o);
                    Groups.Add(r.Read<GroupInfo>());
                    Groups.Last().Index = ind;
                    Groups.Last().Name =
                        ind > (groupNames.Count - 1) ? "GROUP_" + ind : groupNames[ind];
                    for (int i = 0; i < Groups.Last().Entries.Count; i++)
                    {
                        switch (Groups.Last().Entries[i].Type)
                        {
                            case GroupEntryType.Sequence:
                                Groups.Last().Entries[i].Entry = Sequences
                                    .Where(x => x.Index == (int)Groups.Last().Entries[i].ReadingId)
                                    .FirstOrDefault();
                                break;
                            case GroupEntryType.Bank:
                                Groups.Last().Entries[i].Entry = Banks
                                    .Where(x => x.Index == (int)Groups.Last().Entries[i].ReadingId)
                                    .FirstOrDefault();
                                break;
                            case GroupEntryType.WaveArchive:
                                Groups.Last().Entries[i].Entry = WaveArchives
                                    .Where(x => x.Index == (int)Groups.Last().Entries[i].ReadingId)
                                    .FirstOrDefault();
                                break;
                            case GroupEntryType.SequenceArchive:
                                Groups.Last().Entries[i].Entry = SequenceArchives
                                    .Where(x => x.Index == (int)Groups.Last().Entries[i].ReadingId)
                                    .FirstOrDefault();
                                break;
                        }
                    }
                }
                ind++;
            }
            if (invalidWaveErrors.Count > 0)
            {
                string message =
                    "The following wave archives contain invalid wave data (sample rate = 0) and could not be verified:\n\n";
                message += string.Join("\n", invalidWaveErrors);
                message +=
                    "\n\nThe file will continue loading with these unverified waves. Click OK to proceed.";
                _ = MessageBox.Show(
                    message,
                    "Invalid Wave Data Detected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        public override void Write(FileWriter w)
        {
            w.InitFile<SDATHeader>("SDAT", ByteOrder.LittleEndian, null, SaveSymbols ? 4 : 3);
            Sequences = Sequences.OrderBy(x => x.Index).ToList();
            SequenceArchives = SequenceArchives.OrderBy(x => x.Index).ToList();
            Banks = Banks.OrderBy(x => x.Index).ToList();
            WaveArchives = WaveArchives.OrderBy(x => x.Index).ToList();
            Players = Players.OrderBy(x => x.Index).ToList();
            Groups = Groups.OrderBy(x => x.Index).ToList();
            StreamPlayers = StreamPlayers.OrderBy(x => x.Index).ToList();
            Streams = Streams.OrderBy(x => x.Index).ToList();
            if (SaveSymbols)
            {
                w.InitBlock("SYMB", false, true);
                w.Write("SYMB".ToCharArray());
                w.Write((uint)0);
                w.InitOffset("seqStrings");
                w.InitOffset("seqArcStrings");
                w.InitOffset("bnkStrings");
                w.InitOffset("warStrings");
                w.InitOffset("plyStrings");
                w.InitOffset("grpStrings");
                w.InitOffset("stmPlyStrings");
                w.InitOffset("stmStrings");
                w.Write(new uint[6]);
                long prepareStringTable(uint maxEntries)
                {
                    long h = w.Position;
                    w.Write(maxEntries);
                    w.Write(new uint[maxEntries]);
                    return h;
                }
                long seqSBak = 0;
                w.CloseOffset("seqStrings");
                try
                {
                    seqSBak = prepareStringTable((uint)(Sequences.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                w.CloseOffset("seqArcStrings");
                if (SequenceArchives.Count > 1)
                {
                    w.Write((uint)(SequenceArchives.Last().Index + 1));
                    for (int i = 0; i <= SequenceArchives.Last().Index; i++)
                    {
                        if (SequenceArchives.Where(x => x.Index == i).Count() < 1)
                        {
                            continue;
                        }
                        w.InitOffset("seqArcS" + i);
                        w.InitOffset("seqArcSubS" + i);
                    }
                }
                else
                {
                    w.Write((uint)0);
                }
                List<long> seqArcSeqSBak = [];
                if (SequenceArchives.Count > 1)
                {
                    for (int i = 0; i <= SequenceArchives.Last().Index; i++)
                    {
                        if (SequenceArchives.Where(x => x.Index == i).Count() < 1)
                        {
                            seqArcSeqSBak.Add(0);
                            continue;
                        }
                        SequenceArchiveInfo e = SequenceArchives.Where(x => x.Index == i).FirstOrDefault();
                        w.CloseOffset("seqArcSubS" + i);
                        seqArcSeqSBak.Add(w.Position);
                        if (e.File.Sequences.Count > 0)
                        {
                            w.Write((uint)(e.File.Sequences.Last().Index + 1));
                            w.Write(new uint[e.File.Sequences.Last().Index + 1]);
                        }
                        else
                        {
                            w.Write((uint)0);
                        }
                    }
                }
                long bankSBak = 0;
                w.CloseOffset("bnkStrings");
                try
                {
                    bankSBak = prepareStringTable((uint)(Banks.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                long warSBak = 0;
                w.CloseOffset("warStrings");
                try
                {
                    warSBak = prepareStringTable((uint)(WaveArchives.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                long plySBak = 0;
                w.CloseOffset("plyStrings");
                try
                {
                    plySBak = prepareStringTable((uint)(Players.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                long grpSBak = 0;
                w.CloseOffset("grpStrings");
                try
                {
                    grpSBak = prepareStringTable((uint)(Groups.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                long stmPlySBak = 0;
                w.CloseOffset("stmPlyStrings");
                try
                {
                    stmPlySBak = prepareStringTable((uint)(StreamPlayers.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                long stmSBak = 0;
                w.CloseOffset("stmStrings");
                try
                {
                    stmSBak = prepareStringTable((uint)(Streams.Last().Index + 1));
                }
                catch
                {
                    w.Write((uint)0);
                }
                void writeStringData(object entryList, long tablePos)
                {
                    List<string> strgs = [];
                    if (entryList is List<SequenceInfo> seqList)
                    {
                        for (int i = 0; i <= seqList.Last().Index; i++)
                        {
                            IEnumerable<SequenceInfo> y = seqList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    if (entryList is List<BankInfo> bnkList)
                    {
                        for (int i = 0; i <= bnkList.Last().Index; i++)
                        {
                            IEnumerable<BankInfo> y = bnkList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    if (entryList is List<WaveArchiveInfo> warList)
                    {
                        for (int i = 0; i <= warList.Last().Index; i++)
                        {
                            IEnumerable<WaveArchiveInfo> y = warList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    if (entryList is List<PlayerInfo> plyList)
                    {
                        for (int i = 0; i <= plyList.Last().Index; i++)
                        {
                            IEnumerable<PlayerInfo> y = plyList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    if (entryList is List<GroupInfo> grpList)
                    {
                        for (int i = 0; i <= grpList.Last().Index; i++)
                        {
                            IEnumerable<GroupInfo> y = grpList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    if (entryList is List<StreamPlayerInfo> stmPlyList)
                    {
                        for (int i = 0; i <= stmPlyList.Last().Index; i++)
                        {
                            IEnumerable<StreamPlayerInfo> y = stmPlyList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    if (entryList is List<StreamInfo> stmList)
                    {
                        for (int i = 0; i <= stmList.Last().Index; i++)
                        {
                            IEnumerable<StreamInfo> y = stmList.Where(x => x.Index == i);
                            if (y.Count() > 0)
                            {
                                strgs.Add(y.FirstOrDefault().Name);
                            }
                            else
                            {
                                strgs.Add(null);
                            }
                        }
                    }
                    for (int i = 0; i < strgs.Count; i++)
                    {
                        if (strgs[i] == null)
                        {
                            continue;
                        }
                        long bak = w.Position;
                        w.Position = tablePos + 4 + (4 * i);
                        w.Write((uint)(bak - w.CurrentOffset));
                        w.Position = bak;
                        w.WriteNullTerminated(strgs[i]);
                    }
                }
                try
                {
                    writeStringData(Sequences, seqSBak);
                }
                catch { }
                if (SequenceArchives.Count > 0)
                {
                    for (int i = 0; i <= SequenceArchives.Last().Index; i++)
                    {
                        SequenceArchiveInfo e = SequenceArchives.Where(x => x.Index == i).FirstOrDefault();
                        if (e != null)
                        {
                            w.CloseOffset("seqArcS" + i);
                            w.WriteNullTerminated(e.Name);
                            if (e.File.Sequences.Count > 0)
                            {
                                for (int j = 0; j <= e.File.Sequences.Last().Index; j++)
                                {
                                    SequenceArchiveSequence f = e
                                        .File.Sequences.Where(x => x.Index == j)
                                        .FirstOrDefault();
                                    if (f == null)
                                    {
                                        continue;
                                    }
                                    long currPos = w.Position;
                                    w.Position = seqArcSeqSBak[i] + 4 + (4 * f.Index);
                                    w.Write((uint)(currPos - w.CurrentOffset));
                                    w.Position = currPos;
                                    w.WriteNullTerminated(f.Name);
                                }
                            }
                        }
                    }
                }
                try
                {
                    writeStringData(Banks, bankSBak);
                }
                catch { }
                try
                {
                    writeStringData(WaveArchives, warSBak);
                }
                catch { }
                try
                {
                    writeStringData(Players, plySBak);
                }
                catch { }
                try
                {
                    writeStringData(Groups, grpSBak);
                }
                catch { }
                try
                {
                    writeStringData(StreamPlayers, stmPlySBak);
                }
                catch { }
                try
                {
                    writeStringData(Streams, stmSBak);
                }
                catch { }
                long beforePadPosS = w.Position;
                w.Pad(4);
                long afterPadPosS = w.Position;
                w.CloseBlock();
                w.BlockSizes[^1] -= afterPadPosS - beforePadPosS;
            }
            Dictionary<string, Tuple<IOFile, uint>> files =
                [];
            uint fileId = 0;
            foreach (SequenceInfo e in Sequences)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5 + fileId, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else if (!files.ContainsKey(md5))
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else
                {
                    e.ReadingFileId = files[md5].Item2;
                }
            }
            foreach (SequenceArchiveInfo e in SequenceArchives)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5 + fileId, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else if (!files.ContainsKey(md5))
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else
                {
                    e.ReadingFileId = files[md5].Item2;
                }
            }
            foreach (BankInfo e in Banks)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5 + fileId, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else if (!files.ContainsKey(md5))
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else
                {
                    e.ReadingFileId = files[md5].Item2;
                }
            }
            foreach (WaveArchiveInfo e in WaveArchives)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5 + fileId, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else if (!files.ContainsKey(md5))
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else
                {
                    e.ReadingFileId = files[md5].Item2;
                }
            }
            foreach (StreamInfo e in Streams)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5 + fileId, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else if (!files.ContainsKey(md5))
                {
                    e.ReadingFileId = fileId;
                    files.Add(md5, new Tuple<IOFile, uint>(e.File, fileId++));
                }
                else
                {
                    e.ReadingFileId = files[md5].Item2;
                }
            }
            w.InitBlock("INFO");
            w.CurrentOffset -= 8;
            long infoOff = w.Position - 8;
            w.InitOffset("seqInfo");
            w.InitOffset("seqArcInfo");
            w.InitOffset("bnkInfo");
            w.InitOffset("warInfo");
            w.InitOffset("plyInfo");
            w.InitOffset("grpInfo");
            w.InitOffset("stmPlyInfo");
            w.InitOffset("stmInfo");
            w.Write(new uint[6]);
            long prepareInfoTable(uint maxEntries)
            {
                long h = w.Position;
                w.Write(maxEntries);
                w.Write(new uint[maxEntries]);
                return h;
            }
            long seqIBak = 0;
            w.CloseOffset("seqInfo");
            try
            {
                seqIBak = prepareInfoTable((uint)(Sequences.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (Sequences.Count() > 0)
            {
                foreach (SequenceInfo e in Sequences)
                {
                    long bak = w.Position;
                    w.Position = seqIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long seqArcIBak = 0;
            w.CloseOffset("seqArcInfo");
            try
            {
                seqArcIBak = prepareInfoTable((uint)(SequenceArchives.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (SequenceArchives.Count() > 0)
            {
                foreach (SequenceArchiveInfo e in SequenceArchives)
                {
                    long bak = w.Position;
                    w.Position = seqArcIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long bankIBak = 0;
            w.CloseOffset("bnkInfo");
            try
            {
                bankIBak = prepareInfoTable((uint)(Banks.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (Banks.Count() > 0)
            {
                foreach (BankInfo e in Banks)
                {
                    long bak = w.Position;
                    w.Position = bankIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long warIBak = 0;
            w.CloseOffset("warInfo");
            try
            {
                warIBak = prepareInfoTable((uint)(WaveArchives.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (WaveArchives.Count() > 0)
            {
                foreach (WaveArchiveInfo e in WaveArchives)
                {
                    long bak = w.Position;
                    w.Position = warIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long plyIBak = 0;
            w.CloseOffset("plyInfo");
            try
            {
                plyIBak = prepareInfoTable((uint)(Players.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (Players.Count() > 0)
            {
                foreach (PlayerInfo e in Players)
                {
                    long bak = w.Position;
                    w.Position = plyIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long grpIBak = 0;
            w.CloseOffset("grpInfo");
            try
            {
                grpIBak = prepareInfoTable((uint)(Groups.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (Groups.Count() > 0)
            {
                foreach (GroupInfo e in Groups)
                {
                    long bak = w.Position;
                    w.Position = grpIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long stmPlyIBak = 0;
            w.CloseOffset("stmPlyInfo");
            try
            {
                stmPlyIBak = prepareInfoTable((uint)(StreamPlayers.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (StreamPlayers.Count() > 0)
            {
                foreach (StreamPlayerInfo e in StreamPlayers)
                {
                    long bak = w.Position;
                    w.Position = stmPlyIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long stmIBak = 0;
            w.CloseOffset("stmInfo");
            try
            {
                stmIBak = prepareInfoTable((uint)(Streams.Last().Index + 1));
            }
            catch
            {
                w.Write((uint)0);
            }
            if (Streams.Count() > 0)
            {
                foreach (StreamInfo e in Streams)
                {
                    long bak = w.Position;
                    w.Position = stmIBak + 4 + (4 * e.Index);
                    w.Write((uint)(bak - infoOff));
                    w.Position = bak;
                    w.Write(e);
                }
            }
            long beforePadPosI = w.Position;
            w.Pad(4);
            long afterPadPosI = w.Position;
            w.CloseBlock();
            w.BlockSizes[^1] -= afterPadPosI - beforePadPosI;
            w.InitBlock("FAT ");
            List<byte[]> filesRaw = [];
            foreach (KeyValuePair<string, Tuple<IOFile, uint>> f in files)
            {
                filesRaw.Add(f.Value.Item1.Write());
            }
            w.Write((uint)files.Count);
            for (int i = 0; i < filesRaw.Count; i++)
            {
                w.InitOffset("file" + i);
                w.Write((uint)filesRaw[i].Length);
                w.Write((ulong)0);
            }
            long beforePadPosFAT = w.Position;
            w.Pad(4);
            long afterPadPosFAT = w.Position;
            w.CloseBlock();
            w.BlockSizes[^1] -= afterPadPosFAT - beforePadPosFAT;
            w.InitBlock("FILE");
            w.Write((uint)filesRaw.Count);
            w.Pad(0x20);
            for (int i = 0; i < filesRaw.Count; i++)
            {
                w.CloseOffset("file" + i, true);
                w.Write(filesRaw[i]);
                w.Pad(0x20);
            }
            w.CloseBlock();
            w.CloseFile();
        }

        public void ExportSDKProject(string directory, string projectName)
        {
            List<string> sbdl = [];
            foreach (PlayerInfo e in Players)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (WaveArchiveInfo e in WaveArchives)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (StreamPlayerInfo e in StreamPlayers)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (StreamInfo e in Streams)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (BankInfo e in Banks)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (SequenceInfo e in Sequences)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (SequenceArchiveInfo e in SequenceArchives)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            foreach (GroupInfo e in Groups)
            {
                sbdl.Add("#define " + e.Name + "\t" + e.Index);
            }
            File.WriteAllLines(directory + "/" + projectName + ".sbdl", sbdl);
            List<string> sprj =
            [
                "<?xml version=\"1.0\"?>",
                "<NitroSoundMakerProject version=\"1.0.0\">",
                "  <head>",
                "    <create user=\"NitroStudio2User\" host=\"NitroStudio\" date=\"2020 - 3 - 18T12: 37:41\" />"
,
                "    <title>Nitro Studio 2 Export</title>",
                "    <generator name=\"cc\" version=\"1.2.0.0\" />",
                "  </head>",
                "  <body>",
                "    <SoundArchiveFiles>",
                "      <File name=\"" + projectName + "\" path=\"" + projectName + ".sarc\" />"
,
                "    </SoundArchiveFiles>",
                "  </body>",
                "</NitroSoundMakerProject>",
            ];
            File.WriteAllLines(directory + "/" + projectName + ".sprj", sprj);
            Dictionary<int, string> waveFiles = [];
            Dictionary<string, string> waveMd5sums = [];
            foreach (WaveArchiveInfo e in WaveArchives)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    waveFiles.Add(e.Index, e.Name);
                    if (!waveMd5sums.ContainsKey(md5))
                    {
                        waveMd5sums.Add(md5, e.Name);
                    }
                }
                else
                {
                    if (waveMd5sums.ContainsKey(md5))
                    {
                        waveFiles.Add(e.Index, waveMd5sums[md5]);
                    }
                    else
                    {
                        waveFiles.Add(e.Index, e.Name);
                        waveMd5sums.Add(md5, e.Name);
                    }
                }
            }
            Dictionary<int, string> strmFiles = [];
            Dictionary<string, string> strmMd5sums = [];
            foreach (StreamInfo e in Streams)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    strmFiles.Add(e.Index, e.Name);
                    if (!strmMd5sums.ContainsKey(md5))
                    {
                        strmMd5sums.Add(md5, e.Name);
                    }
                }
                else
                {
                    if (strmMd5sums.ContainsKey(md5))
                    {
                        strmFiles.Add(e.Index, strmMd5sums[md5]);
                    }
                    else
                    {
                        strmFiles.Add(e.Index, e.Name);
                        strmMd5sums.Add(md5, e.Name);
                    }
                }
            }
            Dictionary<int, string> bnkFiles = [];
            Dictionary<string, string> bnkMd5sums = [];
            foreach (BankInfo e in Banks)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    bnkFiles.Add(e.Index, e.Name);
                    if (!bnkMd5sums.ContainsKey(md5))
                    {
                        bnkMd5sums.Add(md5, e.Name);
                    }
                }
                else
                {
                    if (bnkMd5sums.ContainsKey(md5))
                    {
                        bnkFiles.Add(e.Index, bnkMd5sums[md5]);
                    }
                    else
                    {
                        bnkFiles.Add(e.Index, e.Name);
                        bnkMd5sums.Add(md5, e.Name);
                    }
                }
            }
            Dictionary<int, string> seqFiles = [];
            Dictionary<string, string> seqMd5sums = [];
            foreach (SequenceInfo e in Sequences)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    seqFiles.Add(e.Index, e.Name);
                    if (!seqMd5sums.ContainsKey(md5))
                    {
                        seqMd5sums.Add(md5, e.Name);
                    }
                }
                else
                {
                    if (seqMd5sums.ContainsKey(md5))
                    {
                        seqFiles.Add(e.Index, seqMd5sums[md5]);
                    }
                    else
                    {
                        seqFiles.Add(e.Index, e.Name);
                        seqMd5sums.Add(md5, e.Name);
                    }
                }
            }
            Dictionary<int, string> seqArcFiles = [];
            Dictionary<string, string> seqArcMd5sums = [];
            foreach (SequenceArchiveInfo e in SequenceArchives)
            {
                string md5 = e.File.Md5Sum;
                if (e.ForceIndividualFile)
                {
                    seqArcFiles.Add(e.Index, e.Name);
                    if (!seqArcMd5sums.ContainsKey(md5))
                    {
                        seqArcMd5sums.Add(md5, e.Name);
                    }
                }
                else
                {
                    if (seqArcMd5sums.ContainsKey(md5))
                    {
                        seqArcFiles.Add(e.Index, seqArcMd5sums[md5]);
                    }
                    else
                    {
                        seqArcFiles.Add(e.Index, e.Name);
                        seqArcMd5sums.Add(md5, e.Name);
                    }
                }
            }
            List<string> sarc = [];
            int id = 0;
            sarc.Add("@PLAYER");
            foreach (PlayerInfo e in Players)
            {
                ushort bitFlags = e.BitFlags();
                if (bitFlags == 0xFFFF)
                {
                    bitFlags = 0;
                }
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                sarc.Add(
                    e.Name
                        + index
                        + "\t: "
                        + e.SequenceMax
                        + ", "
                        + e.HeapSize
                        + ", 0x"
                        + bitFlags.ToString("X")
                );
            }
            id = 0;
            sarc.Add("\n@WAVEARC\n\n @PATH \"WaveArchives\"");
            foreach (WaveArchiveInfo e in WaveArchives)
            {
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                sarc.Add(
                    e.Name
                        + index
                        + "\t: TEXT, \""
                        + waveFiles[e.Index]
                        + ".swls\""
                        + (e.LoadIndividually ? ", s" : "")
                );
            }
            id = 0;
            sarc.Add("\n@BANK\n\n @PATH \"Banks\"");
            foreach (BankInfo e in Banks)
            {
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                bool text = true;
                try
                {
                    _ = Directory.CreateDirectory("TEMP");
                    e.WriteTextFormat("TEMP", "Test");
                    Directory.Delete("TEMP", true);
                }
                catch
                {
                    text = false;
                }
                string stuff =
                    e.Name
                    + index
                    + "\t: "
                    + (text ? "TEXT" : "BIN")
                    + ", \""
                    + bnkFiles[e.Index]
                    + "."
                    + (text ? "" : "s")
                    + "bnk\""
                    + ", ";
                string[] wars = new string[4];
                for (int i = 0; i < wars.Length; i++)
                {
                    if (e.WaveArchives[i] == null)
                    {
                        switch (i)
                        {
                            case 0:
                                if (e.ReadingWave0Id != 0xFFFF)
                                {
                                    wars[i] = e.ReadingWave0Id.ToString();
                                }
                                break;
                            case 1:
                                if (e.ReadingWave1Id != 0xFFFF)
                                {
                                    wars[i] = e.ReadingWave1Id.ToString();
                                }
                                break;
                            case 2:
                                if (e.ReadingWave2Id != 0xFFFF)
                                {
                                    wars[i] = e.ReadingWave2Id.ToString();
                                }
                                break;
                            case 3:
                                if (e.ReadingWave3Id != 0xFFFF)
                                {
                                    wars[i] = e.ReadingWave3Id.ToString();
                                }
                                break;
                        }
                    }
                    else
                    {
                        wars[i] = e.WaveArchives[i].Name;
                    }
                }
                if (wars[0] != null)
                {
                    stuff += wars[0];
                }
                if (wars[1] != null || wars[2] != null || wars[3] != null)
                {
                    stuff += ", ";
                }
                if (wars[1] != null)
                {
                    stuff += wars[1];
                }
                if (wars[2] != null || wars[3] != null)
                {
                    stuff += ", ";
                }
                if (wars[2] != null)
                {
                    stuff += wars[2];
                }
                if (wars[3] != null)
                {
                    stuff += ", ";
                }
                if (wars[3] != null)
                {
                    stuff += wars[3];
                }
                sarc.Add(stuff);
            }
            id = 0;
            sarc.Add("\n@SEQ\n\n @PATH \"Sequences\"");
            foreach (SequenceInfo e in Sequences)
            {
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                sarc.Add(
                    e.Name
                        + index
                        + "\t: TEXT, \""
                        + seqFiles[e.Index]
                        + ".smft\", "
                        + (e.Bank == null ? e.ReadingBankId.ToString() : e.Bank.Name)
                        + ", "
                        + e.Volume
                        + ", "
                        + e.ChannelPriority
                        + ", "
                        + e.PlayerPriority
                        + ", "
                        + (e.Player == null ? e.ReadingPlayerId.ToString() : e.Player.Name)
                );
            }
            id = 0;
            sarc.Add("\n@SEQARC\n\n @PATH \"SequenceArchives\"");
            foreach (SequenceArchiveInfo e in SequenceArchives)
            {
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                sarc.Add(e.Name + index + "\t: TEXT, \"" + seqArcFiles[e.Index] + ".mus\"");
            }
            id = 0;
            sarc.Add("\n@STRM_PLAYER");
            foreach (StreamPlayerInfo e in StreamPlayers)
            {
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                sarc.Add(
                    e.Name
                        + index
                        + "\t: "
                        + (e.IsStereo ? "STEREO" : "MONO")
                        + ", "
                        + e.LeftChannel
                        + (e.IsStereo ? ", " + e.RightChannel : "")
                );
            }
            id = 0;
            sarc.Add("\n@STRM\n\n @PATH \"Streams\"");
            foreach (StreamInfo e in Streams)
            {
                string index = "";
                if (id != e.Index)
                {
                    id = e.Index;
                    index = "\t= " + e.Index;
                }
                id++;
                sarc.Add(
                    e.Name
                        + index
                        + "\t: "
                        + "STRM"
                        + ", \""
                        + strmFiles[e.Index]
                        + ".strm\", "
                        + e.Volume
                        + ", "
                        + e.Priority
                        + ", "
                        + (e.Player == null ? e.ReadingPlayerId.ToString() : e.Player.Name)
                );
            }
            sarc.Add("\n@GROUP");
            foreach (GroupInfo e in Groups)
            {
                sarc.Add(e.Name + "\t:");
                foreach (GroupEntry t in e.Entries)
                {
                    string stuff = "  ";
                    switch (t.Type)
                    {
                        case GroupEntryType.Sequence:
                            stuff += (t.Entry as SequenceInfo).Name;
                            break;
                        case GroupEntryType.Bank:
                            stuff += (t.Entry as BankInfo).Name;
                            break;
                        case GroupEntryType.WaveArchive:
                            stuff += (t.Entry as WaveArchiveInfo).Name;
                            break;
                        case GroupEntryType.SequenceArchive:
                            stuff += (t.Entry as SequenceArchiveInfo).Name;
                            break;
                    }
                    bool sseq = t.LoadSequence,
                        sbnk = t.LoadBank,
                        swar = t.LoadWaveArchive;
                    switch (t.Type)
                    {
                        case GroupEntryType.Sequence:
                            if (sseq && sbnk && swar) { }
                            else if (sbnk && swar)
                            {
                                stuff += ", bw";
                            }
                            else if (sseq && swar)
                            {
                                stuff += ", sw";
                            }
                            else if (swar)
                            {
                                stuff += ", w";
                            }
                            else if (sbnk)
                            {
                                stuff += ", b";
                            }
                            else if (sseq)
                            {
                                stuff += ", s";
                            }
                            break;
                        case GroupEntryType.Bank:
                            if (sbnk && swar)
                            {
                                stuff += ", bw";
                            }
                            else if (swar)
                            {
                                stuff += ", w";
                            }
                            else if (sbnk)
                            {
                                stuff += ", b";
                            }
                            break;
                    }
                    sarc.Add(stuff);
                }
                sarc.Add("");
            }
            File.WriteAllLines(directory + "/" + projectName + ".sarc", sarc);
            List<string> wWavs = [];
            foreach (WaveArchiveInfo e in WaveArchives)
            {
                _ = Directory.CreateDirectory(directory + "/" + "WaveArchives");
                if (!wWavs.Contains(waveFiles[e.Index]))
                {
                    e.WriteTextFormat(directory + "/WaveArchives", waveFiles[e.Index]);
                    wWavs.Add(waveFiles[e.Index]);
                }
            }
            List<string> wStrms = [];
            foreach (StreamInfo e in Streams)
            {
                _ = Directory.CreateDirectory(directory + "/" + "Streams");
                if (!wStrms.Contains(strmFiles[e.Index]))
                {
                    e.File.Write(directory + "/" + "Streams" + "/" + strmFiles[e.Index] + ".strm");
                    RiffWave r = new();
                    r.FromOtherStreamFile(e.File);
                    r.Write(directory + "/" + "Streams" + "/" + strmFiles[e.Index] + ".wav");
                    wStrms.Add(strmFiles[e.Index]);
                }
            }
            List<string> wSeqs = [];
            foreach (SequenceInfo e in Sequences)
            {
                _ = Directory.CreateDirectory(directory + "/" + "Sequences");
                if (!wSeqs.Contains(seqFiles[e.Index]))
                {
                    e.File.Name = seqFiles[e.Index];
                    e.File.ReadCommandData();
                    File.WriteAllLines(
                        directory + "/" + "Sequences" + "/" + seqFiles[e.Index] + ".smft",
                        e.File.ToText()
                    );
                    wSeqs.Add(seqFiles[e.Index]);
                }
            }
            List<string> wSeqArcs = [];
            foreach (SequenceArchiveInfo e in SequenceArchives)
            {
                _ = Directory.CreateDirectory(directory + "/" + "SequenceArchives");
                if (!wSeqArcs.Contains(seqArcFiles[e.Index]))
                {
                    e.File.Name = seqArcFiles[e.Index];
                    e.File.ReadCommandData(true);
                    List<string> l = e.File.ToText().ToList();
                    l.Insert(0, "#include \"../" + projectName + ".sbdl\"\n");
                    File.WriteAllLines(
                        directory + "/" + "SequenceArchives" + "/" + seqArcFiles[e.Index] + ".mus",
                        l
                    );
                    wSeqArcs.Add(seqArcFiles[e.Index]);
                }
            }
            List<string> wBnks = [];
            foreach (BankInfo e in Banks)
            {
                _ = Directory.CreateDirectory(directory + "/" + "Banks");
                if (!wBnks.Contains(bnkFiles[e.Index]))
                {
                    try
                    {
                        e.WriteTextFormat(directory + "/Banks", bnkFiles[e.Index]);
                        wBnks.Add(e.Name);
                    }
                    catch { }
                }
            }
        }
    }
}
