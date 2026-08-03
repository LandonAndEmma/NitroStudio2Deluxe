using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroStudio2.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Adding, swapping, renaming and deleting archive entries. Ported from the corresponding
    /// MainWindow handlers; the eight-way switches each of them carried are driven by the
    /// <see cref="Category"/> table instead.
    /// </summary>
    public sealed partial class SoundArchiveViewModel
    {
        /// <summary>Rebuilds the tree and reselects the entry with the given id in a category.</summary>
        private void RefreshAndSelect(string categoryKey, int id)
        {
            UpdateNodes();
            EditorTreeNode root = Nodes.FirstOrDefault(n => n.Name == categoryKey);
            EditorTreeNode found = root?.Nodes.FirstOrDefault(n => n.Text.Contains("[" + id + "]"));
            if (found is not null)
            {
                found.ExpandPath();
                SelectedNode = found;
            }
            DoInfoStuff();
        }

        // ------------------------------------------------------------------ id allocation

        private bool RootHasId(string categoryKey, int id)
        {
            return Nodes.First(n => n.Name == categoryKey)
                .Nodes.Any(n => n.Text.Contains("[" + id + "]"));
        }

        /// <summary>First free id at or after the preferred one, wrapping to 0. -1 when full.</summary>
        private async Task<int> NextAvailableForwardIdAsync(
            int preferredId,
            uint maxId,
            string categoryKey
        )
        {
            int id = preferredId;
            while (id <= maxId && RootHasId(categoryKey, id))
            {
                id++;
            }
            if (id > maxId)
            {
                id = 0;
                while (id < preferredId && RootHasId(categoryKey, id))
                {
                    id++;
                }
                if (id == preferredId)
                {
                    await Dialogs.ShowMessageAsync("There are no more available slots for the item!");
                    return -1;
                }
            }
            return id < 0 ? -1 : id;
        }

        /// <summary>First free id at or before the preferred one, wrapping to maxId. -1 when full.</summary>
        private async Task<int> NextAvailablePreviousIdAsync(
            int preferredId,
            uint maxId,
            string categoryKey
        )
        {
            int id = preferredId;
            while (id >= 0 && RootHasId(categoryKey, id))
            {
                id--;
            }
            if (id < 0)
            {
                id = (int)maxId;
                while (id > preferredId && RootHasId(categoryKey, id))
                {
                    id--;
                }
                if (id == preferredId)
                {
                    await Dialogs.ShowMessageAsync("There are no more available slots for the item!");
                    return -1;
                }
            }
            return id < 0 ? -1 : id;
        }

        // ------------------------------------------------------------------ add

        public override void NodeAddAbove()
        {
            _ = AddRelativeAsync(above: true);
        }

        public override void NodeAddBelow()
        {
            _ = AddRelativeAsync(above: false);
        }

        private async Task AddRelativeAsync(bool above)
        {
            Category category = Selected();
            if (category is null)
            {
                return;
            }
            int preferred = IdFromNode(SelectedNode);
            int id = above
                ? await NextAvailablePreviousIdAsync(preferred, category.MaxId, category.Key)
                : await NextAvailableForwardIdAsync(preferred, category.MaxId, category.Key);
            if (id == -1)
            {
                return;
            }
            category.Insert(id);
            category.Sort();
            RefreshAndSelect(category.Key, id);
        }

        public override void RootAdd()
        {
            _ = RootAddAsync();
        }

        /// <summary>Adds an entry to whichever category root is selected, at the first free id.</summary>
        private async Task RootAddAsync()
        {
            if (
                SelectedNode is null
                || !categories.TryGetValue(SelectedNode.Name, out Category category)
            )
            {
                return;
            }
            int id = await NextAvailableForwardIdAsync(0, category.MaxId, category.Key);
            if (id == -1)
            {
                return;
            }
            category.Insert(id);
            category.Sort();
            RefreshAndSelect(category.Key, id);
        }

        // ------------------------------------------------------------------ entry factories

        private void AddSequence(int index)
        {
            _ = AddSequenceAsync(index);
        }

        private async Task AddSequenceAsync(int index)
        {
            if (SA.Banks.Count < 1)
            {
                await Dialogs.ShowMessageAsync(
                    "There must be at least one bank in order to add a sequence."
                );
                return;
            }
            if (SA.Players.Count < 1)
            {
                await Dialogs.ShowMessageAsync(
                    "There must be at least one sequence player in order to add a sequence."
                );
                return;
            }
            SequenceInfo e = new()
            {
                Bank = SA.Banks[0],
                Player = SA.Players[0],
                Name = UniqueName("SEQ_", index, n => SA.Sequences.Any(x => x.Name == n)),
                Index = index,
                File = new Sequence { RawData = [0xFF], Labels = [] },
            };
            SA.Sequences.Add(e);
        }

        private void AddSequenceArchive(int index)
        {
            _ = AddSequenceArchiveAsync(index);
        }

        private async Task AddSequenceArchiveAsync(int index)
        {
            if (SA.Banks.Count < 1)
            {
                await Dialogs.ShowMessageAsync(
                    "There must be at least one bank in order to add a sequence archive."
                );
                return;
            }
            if (SA.Players.Count < 1)
            {
                await Dialogs.ShowMessageAsync(
                    "There must be at least one sequence player in order to add a sequence archive."
                );
                return;
            }
            SA.SequenceArchives.Add(
                new SequenceArchiveInfo
                {
                    // The original checked Sequences, not SequenceArchives, for the name clash.
                    Name = UniqueName("SEQARC_", index, n => SA.Sequences.Any(x => x.Name == n)),
                    Index = index,
                    File = new SequenceArchive { RawData = [], Labels = [] },
                }
            );
        }

        private void AddBank(int index)
        {
            SA.Banks.Add(
                new BankInfo
                {
                    File = new Bank(),
                    Name = UniqueName("BANK_", index, n => SA.Banks.Any(x => x.Name == n)),
                    Index = index,
                }
            );
        }

        private void AddWaveArchive(int index)
        {
            SA.WaveArchives.Add(
                new WaveArchiveInfo
                {
                    File = new WaveArchive(),
                    Name = UniqueName("WAR_", index, n => SA.WaveArchives.Any(x => x.Name == n)),
                    Index = index,
                }
            );
        }

        private void AddSequencePlayer(int index)
        {
            SA.Players.Add(
                new PlayerInfo
                {
                    Name = UniqueName("PLAYER_", index, n => SA.Players.Any(x => x.Name == n)),
                    Index = index,
                    ChannelFlags = Enumerable.Repeat(true, 16).ToArray(),
                }
            );
        }

        private void AddGroup(int index)
        {
            SA.Groups.Add(
                new GroupInfo
                {
                    Name = UniqueName("GROUP_", index, n => SA.Groups.Any(x => x.Name == n)),
                    Index = index,
                    Entries = [],
                }
            );
        }

        private void AddStreamPlayer(int index)
        {
            SA.StreamPlayers.Add(
                new StreamPlayerInfo
                {
                    Name = UniqueName(
                        "STRM_PLAYER_",
                        index,
                        n => SA.StreamPlayers.Any(x => x.Name == n)
                    ),
                    Index = index,
                }
            );
        }

        private void AddStream(int index)
        {
            _ = AddStreamAsync(index);
        }

        private async Task AddStreamAsync(int index)
        {
            if (SA.StreamPlayers.Count < 1)
            {
                await Dialogs.ShowMessageAsync(
                    "The must be at least one stream player in order to add a stream."
                );
                return;
            }
            string path = await Dialogs.OpenFileAsync("Supported Audio Files|*.wav;*.swav;*.strm");
            if (path == "")
            {
                return;
            }
            NitroFileLoader.Stream s = new();
            switch (Path.GetExtension(path))
            {
                case ".wav":
                    RiffWave r = new();
                    r.Read(path);
                    s.FromOtherStreamFile(r);
                    break;
                case ".swav":
                    Wave w = new();
                    w.Read(path);
                    s.FromOtherStreamFile(w);
                    break;
                case ".strm":
                    s.Read(path);
                    break;
            }
            SA.Streams.Add(
                new StreamInfo
                {
                    Name = UniqueName("STRM_", index, n => SA.Streams.Any(x => x.Name == n)),
                    Index = index,
                    Player = SA.StreamPlayers[0],
                    File = s,
                }
            );
            SA.Streams = [.. SA.Streams.OrderBy(x => x.Index)];
            RefreshAndSelect("streams", index);
        }

        /// <summary>
        /// "PREFIX_3", bumping the suffix while the name is taken. Mirrors the original loop,
        /// which kept the first index in the name until a clash forced it up.
        /// </summary>
        private static string UniqueName(string prefix, int index, Func<string, bool> taken)
        {
            string name = prefix + index;
            int nameIndex = index;
            while (taken(prefix + nameIndex))
            {
                name = prefix + nameIndex++;
            }
            return name;
        }

        // ------------------------------------------------------------------ swap

        /// <summary>Swaps the selected entry with whatever sits at the typed index.</summary>
        private async Task SwapAtIndexAsync()
        {
            Category category = Selected();
            if (category is null || !FileOpen || File is null)
            {
                return;
            }
            int index = (int)IndexPanel.ItemIndex;
            int previousIndex = IdFromNode(SelectedNode);
            if ((uint)index > category.MaxId)
            {
                await Dialogs.ShowMessageAsync("Index is outside the max possible Id!");
            }

            object occupant = category.Items().FirstOrDefault(x => category.GetIndex(x) == index);
            object moving = category.Items().FirstOrDefault(x => category.GetIndex(x) == previousIndex);
            if (moving is null)
            {
                return;
            }
            category.SetIndex(moving, index);
            if (occupant is not null)
            {
                category.SetIndex(occupant, previousIndex);
            }
            category.Sort();
            RefreshAndSelect(category.Key, index);
        }

        // ------------------------------------------------------------------ rename

        private async Task RenameAsync()
        {
            if (SelectedNode is null)
            {
                return;
            }

            // A sequence inside a sequence archive renames within that archive.
            if (SelectedNode.Parent?.Parent is not null)
            {
                SequenceArchiveSequence inner = SA
                    .SequenceArchives.First(x => x.Index == IdFromNode(SelectedNode.Parent))
                    .File.Sequences.First(x => x.Index == IdFromNode(SelectedNode));
                string innerName = await Dialogs.InputBoxAsync(
                    "Rename the entry:",
                    "Renamer",
                    inner.Name
                );
                if (innerName == "")
                {
                    return;
                }
                inner.Name = innerName;
                UpdateNodes();
                DoInfoStuff();
                return;
            }

            Category category = Selected();
            if (category is null)
            {
                return;
            }
            object entry = SelectedEntry();
            string name = await Dialogs.InputBoxAsync(
                "Rename the entry:",
                "Renamer",
                category.GetName(entry)
            );
            if (name == "")
            {
                return;
            }
            category.SetName(entry, name);
            RefreshAndSelect(category.Key, category.GetIndex(entry));
        }

        // ------------------------------------------------------------------ delete

        private async Task DeleteAsync()
        {
            Category category = Selected();
            if (category is null)
            {
                return;
            }
            object entry = SelectedEntry();
            if (entry is null)
            {
                return;
            }
            category.Remove(entry);
            category.Sort();
            UpdateNodes();
            SelectedNode = Nodes.First(n => n.Name == category.Key);
            DoInfoStuff();
            await Task.CompletedTask;
        }
    }
}
