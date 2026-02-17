using GotaSequenceLib.Playback;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NitroStudio2
{
    public partial class BankGenerator : Form
    {
        public SoundArchive SA => MainWindow.SA;
        public MainWindow MainWindow;
        private bool WritingInfo;
        public Player Player;
        public Mixer Mixer = new();

        public BankGenerator(MainWindow m)
        {
            InitializeComponent();
            MainWindow = m;
            if (SA.Banks.Where(x => x.File.Instruments.Count > 0).Count() < 1)
            {
                _ = MessageBox.Show("There must be at least one bank that has an instrument.");
                Close();
                return;
            }
            PopulateBankBox(SA, instruments.Columns["bank"] as DataGridViewComboBoxColumn);
            instruments.CellValueChanged += new DataGridViewCellEventHandler(InstrumentsChanged);
            instruments.RowsRemoved += new DataGridViewRowsRemovedEventHandler(InstrumentsChanged);
            instruments.CellContentClick += new DataGridViewCellEventHandler(PlayRegionButtonClick);
            Player = new Player(Mixer);
            FormClosing += new FormClosingEventHandler(OnClosing);
        }

        public static void PopulateBankBox(SoundArchive a, DataGridViewComboBoxColumn c)
        {
            c.Items.Clear();
            foreach (BankInfo w in a.Banks.Where(x => x.File.Instruments.Count > 0))
            {
                _ = c.Items.Add("[" + w.Index + "] - " + w.Name);
            }
        }

        public static void PopulateInstrumentBox(
            Bank b,
            DataGridViewComboBoxCell c,
            bool setFirst = false
        )
        {
            if (!setFirst)
            {
                c.Items.Clear();
                foreach (Instrument i in b.Instruments)
                {
                    _ = c.Items.Add("[" + i.Index + "] - " + i.Type().ToString());
                }
            }
            else
            {
                c.Items[0] =
                    "[" + b.Instruments[0].Index + "] - " + b.Instruments[0].Type().ToString();
                for (int i = 1; i < b.Instruments.Count; i++)
                {
                    _ = c.Items.Add(
                        "[" + b.Instruments[i].Index + "] - " + b.Instruments[i].Type().ToString()
                    );
                }
            }
        }

        public void InstrumentsChanged(object sender, EventArgs e)
        {
            if (WritingInfo)
            {
                return;
            }
            WritingInfo = true;
            List<int> ids = [-1];
            for (int i = 1; i < instruments.Rows.Count; i++)
            {
                DataGridViewComboBoxCell bankCell = (DataGridViewComboBoxCell)instruments.Rows[i - 1].Cells["bank"];
                DataGridViewComboBoxCell instCell = (DataGridViewComboBoxCell)
                    instruments.Rows[i - 1].Cells["instrument"];
                DataGridViewTextBoxCell idCell = (DataGridViewTextBoxCell)instruments.Rows[i - 1].Cells["newId"];
                DataGridViewComboBoxCell warModeCell = (DataGridViewComboBoxCell)
                    instruments.Rows[i - 1].Cells["waveArchiveMode"];
                bankCell.Value ??= (
                        instruments.Columns["bank"] as DataGridViewComboBoxColumn
                    ).Items[0];
                if (instCell.Items.Count < 1)
                {
                    PopulateInstrumentBox(
                        SA.Banks.Where(x =>
                                x.Index
                                == int.Parse(((string)bankCell.Value).Split('[')[1].Split(']')[0])
                            )
                            .FirstOrDefault()
                            .File,
                        instCell,
                        false
                    );
                }
                string instBak = "";
                try
                {
                    instBak = (string)instCell.Value;
                }
                catch { }
                instCell.Value = instCell.Items[0];
                while (instCell.Items.Count > 1)
                {
                    instCell.Items.RemoveAt(instCell.Items.Count - 1);
                }
                instBak ??= "";
                PopulateInstrumentBox(
                    SA.Banks.Where(x =>
                            x.Index
                            == int.Parse(((string)bankCell.Value).Split('[')[1].Split(']')[0])
                        )
                        .FirstOrDefault()
                        .File,
                    instCell,
                    true
                );
                instCell.Value = instCell.Items.Contains(instBak) ? instBak : instCell.Items[0];
                if (idCell.Value == null || idCell.Value.ToString() == "")
                {
                    int newId = ids.Last() + 1;
                    while (ids.Contains(newId))
                    {
                        newId++;
                    }
                    idCell.Value = newId;
                    ids.Add(newId);
                }
                else
                {
                    if (ids.Contains(int.Parse(idCell.Value.ToString())))
                    {
                        int newId = ids.Last() + 1;
                        while (ids.Contains(newId))
                        {
                            newId++;
                        }
                        idCell.Value = newId;
                        ids.Add(newId);
                    }
                    ids.Add(int.Parse(idCell.Value.ToString()));
                }
                warModeCell.Value ??= (
                        instruments.Columns["waveArchiveMode"] as DataGridViewComboBoxColumn
                    ).Items[0];
                bool warModeSecond = warModeCell.Value != warModeCell.Items[0];
                BankInfo b = SA
                    .Banks.Where(x =>
                        x.Index == int.Parse(((string)bankCell.Value).Split('[')[1].Split(']')[0])
                    )
                    .FirstOrDefault();
                Instrument inst = b
                    .File.Instruments.Where(x =>
                        x.Index == int.Parse(((string)instCell.Value).Split('[')[1].Split(']')[0])
                    )
                    .FirstOrDefault();
                List<string> wars = [];
                foreach (NoteInfo n in inst.NoteInfo)
                {
                    if (n.InstrumentType == NitroFileLoader.Instrument.InstrumentType.PCM)
                    {
                        string name = "Null";
                        try
                        {
                            name = b.WaveArchives[n.WarId].Name;
                        }
                        catch { }
                        if (!wars.Contains(name))
                        {
                            wars.Add(name);
                        }
                    }
                }
                string newWarModeName =
                    "Use Existing Wave Archive" + (wars.Count > 1 ? "s" : "") + " " + "(";
                if (wars.Count < 1)
                {
                    newWarModeName += "None)";
                }
                else
                {
                    for (int j = 0; j < wars.Count - 1; j++)
                    {
                        newWarModeName += wars[j] + ", ";
                    }
                    newWarModeName += wars.Last() + ")";
                }
                warModeCell.Items[1] = newWarModeName;
                if (warModeSecond)
                {
                    warModeCell.Value = warModeCell.Items[1];
                }
            }
            WritingInfo = false;
        }

        private void CreateBnk_Click(object sender, EventArgs e)
        {
            BankInfo bnk = new() { File = new Bank() };
            WaveArchiveInfo war = new() { File = new WaveArchive() };
            bool usesGen;
            List<InstrumentInfo> insts = [];
            List<string> wars = [];
            for (int i = 0; i < instruments.Rows.Count - 1; i++)
            {
                DataGridViewComboBoxCell bankCell = (DataGridViewComboBoxCell)instruments.Rows[i].Cells["bank"];
                DataGridViewComboBoxCell instCell = (DataGridViewComboBoxCell)instruments.Rows[i].Cells["instrument"];
                DataGridViewTextBoxCell idCell = (DataGridViewTextBoxCell)instruments.Rows[i].Cells["newId"];
                DataGridViewComboBoxCell warModeCell = (DataGridViewComboBoxCell)
                    instruments.Rows[i].Cells["waveArchiveMode"];
                if (
                    bankCell.Value == null
                    || instCell.Value == null
                    || idCell.Value == null
                    || !int.TryParse(idCell.Value.ToString(), out _)
                    || warModeCell.Value == null
                )
                {
                    _ = MessageBox.Show("Grid contains invalid data.");
                    return;
                }
                BankInfo b = SA
                    .Banks.Where(x =>
                        x.Index == int.Parse(((string)bankCell.Value).Split('[')[1].Split(']')[0])
                    )
                    .FirstOrDefault();
                Instrument inst = b
                    .File.Instruments.Where(x =>
                        x.Index == int.Parse(((string)instCell.Value).Split('[')[1].Split(']')[0])
                    )
                    .FirstOrDefault();
                int id = int.Parse(idCell.Value.ToString());
                bool useExistingWar =
                    warModeCell.Value
                    != ((DataGridViewComboBoxColumn)instruments.Columns["waveArchiveMode"]).Items[
                        0
                    ];
                insts.Add(
                    new InstrumentInfo()
                    {
                        Bank = b,
                        Inst = Bank.DuplicateInstrument(inst),
                        NewId = id,
                        UseExistingWar = useExistingWar,
                    }
                );
                foreach (NoteInfo n in inst.NoteInfo)
                {
                    if (
                        warModeCell.Value != warModeCell.Items[0]
                        && n.InstrumentType == NitroFileLoader.Instrument.InstrumentType.PCM
                    )
                    {
                        string name = "Null";
                        try
                        {
                            name = b.WaveArchives[n.WarId].Name;
                        }
                        catch { }
                        if (!wars.Contains(name) && !name.Equals("Null"))
                        {
                            wars.Add(name);
                        }
                    }
                }
            }
            if (wars.Count > 4)
            {
                _ = MessageBox.Show(
                    "You can't generate a new bank that uses more than 4 wave archives."
                );
                return;
            }
            usesGen = insts.Where(x => x.UseExistingWar == false).Count() > 0;
            if (wars.Count > 3)
            {
                _ = MessageBox.Show(
                    "You can't generate a new bank that uses more than 3 wave archives when creating a generated wave archive."
                );
                return;
            }
            try
            {
                bnk.Index = SA.Banks.Last().Index + 1;
            }
            catch { }
            while (SA.Banks.Where(x => x.Index == bnk.Index).Count() > 0)
            {
                bnk.Index++;
            }
            try
            {
                war.Index = SA.WaveArchives.Last().Index + 1;
            }
            catch { }
            while (SA.WaveArchives.Where(x => x.Index == war.Index).Count() > 0)
            {
                war.Index++;
            }
            bnk.Name = "GENERATED_BANK_" + bnk.Index;
            war.Name = "GENERATED_WAR_" + war.Index;
            Dictionary<ushort, ushort> warLinks = [];
            if (usesGen)
            {
                warLinks.Add((ushort)war.Index, (ushort)warLinks.Count);
            }
            Dictionary<uint, ushort> swavLinks = [];
            ushort swarNum = usesGen ? (ushort)1 : (ushort)0;
            ushort swavNum = 0;
            foreach (InstrumentInfo i in insts)
            {
                foreach (
                    NoteInfo n in i.Inst.NoteInfo.Where(x => x.InstrumentType == NitroFileLoader.Instrument.InstrumentType.PCM)
                )
                {
                    uint hash;
                    try
                    {
                        hash = (uint)(i.Bank.WaveArchives[n.WarId].Index << 16) | n.WaveId;
                    }
                    catch
                    {
                        continue;
                    }
                    if (i.UseExistingWar)
                    {
                        if (!warLinks.ContainsKey((ushort)i.Bank.WaveArchives[n.WarId].Index))
                        {
                            warLinks.Add((ushort)i.Bank.WaveArchives[n.WarId].Index, swarNum++);
                        }
                        n.WarId = warLinks[(ushort)i.Bank.WaveArchives[n.WarId].Index];
                    }
                    else
                    {
                        if (!swavLinks.ContainsKey(hash))
                        {
                            try
                            {
                                war.File.Waves.Add(
                                    i.Bank.WaveArchives[n.WarId].File.Waves[n.WaveId]
                                );
                                swavLinks.Add(hash, swavNum++);
                            }
                            catch { }
                        }
                        n.WarId = 0;
                        n.WaveId = swavLinks[hash];
                    }
                }
                i.Inst.Index = i.NewId;
                bnk.File.Instruments.Add(i.Inst);
            }
            if (warLinks.Count() > 4)
            {
                _ = MessageBox.Show(
                    "Something went wrong, and the max number of wave archives (4) has been exceeded."
                );
                return;
            }
            int bnkWarId = 0;
            foreach (KeyValuePair<ushort, ushort> w in warLinks)
            {
                switch (bnkWarId)
                {
                    case 0:
                        bnk.WaveArchives[0] = SA
                            .WaveArchives.Where(x => x.Index == w.Key)
                            .FirstOrDefault();
                        bnk.ReadingWave0Id = w.Key;
                        break;
                    case 1:
                        bnk.WaveArchives[1] = SA
                            .WaveArchives.Where(x => x.Index == w.Key)
                            .FirstOrDefault();
                        bnk.ReadingWave1Id = w.Key;
                        break;
                    case 2:
                        bnk.WaveArchives[2] = SA
                            .WaveArchives.Where(x => x.Index == w.Key)
                            .FirstOrDefault();
                        bnk.ReadingWave2Id = w.Key;
                        break;
                    case 3:
                        bnk.WaveArchives[3] = SA
                            .WaveArchives.Where(x => x.Index == w.Key)
                            .FirstOrDefault();
                        bnk.ReadingWave3Id = w.Key;
                        break;
                }
                bnkWarId++;
            }
            SA.Banks.Add(bnk);
            if (usesGen)
            {
                SA.WaveArchives.Add(war);
            }
            Close();
            MainWindow.UpdateNodes();
            MainWindow.DoInfoStuff();
        }

        public void PlayRegionButtonClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0)
            {
                return;
            }
            try
            {
                Player.Stop();
                DataGridViewComboBoxCell bnkCell = (DataGridViewComboBoxCell)instruments.Rows[e.RowIndex].Cells["bank"];
                DataGridViewComboBoxCell instCell = (DataGridViewComboBoxCell)
                    instruments.Rows[e.RowIndex].Cells["instrument"];
                BankInfo bnk = SA
                    .Banks.Where(x =>
                        x.Index == int.Parse(((string)bnkCell.Value).Split('[')[1].Split(']')[0])
                    )
                    .FirstOrDefault();
                Instrument inst = bnk
                    .File.Instruments.Where(x =>
                        x.Index == int.Parse(((string)instCell.Value).Split('[')[1].Split(']')[0])
                    )
                    .FirstOrDefault();
                Player.PrepareForSong(new PlayableBank[] { bnk.File }, bnk.GetAssociatedWaves());
                Player.LoadSong(
                    [
                        new GotaSequenceLib.SequenceCommand()
                        {
                            CommandType = GotaSequenceLib.SequenceCommands.ProgramChange,
                            Parameter = (uint)inst.Index,
                        },
                        new GotaSequenceLib.SequenceCommand()
                        {
                            CommandType = GotaSequenceLib.SequenceCommands.Note,
                            Parameter = new GotaSequenceLib.NoteParameter()
                            {
                                Note = GotaSequenceLib.Notes.cn4,
                                Length = 48 * 2,
                                Velocity = 127,
                            },
                        },
                        new GotaSequenceLib.SequenceCommand()
                        {
                            CommandType = GotaSequenceLib.SequenceCommands.Fin,
                        },
                    ]
                );
                Player.Play();
            }
            catch { }
        }

        private void OnClosing(object sender, EventArgs e)
        {
            Mixer.Dispose();
            Player.Dispose();
        }

        public struct InstrumentInfo
        {
            public BankInfo Bank;
            public Instrument Inst;
            public int NewId;
            public bool UseExistingWar;
        }
    }
}
