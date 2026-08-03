using GotaSequenceLib;
using GotaSoundBank.DLS;
using GotaSoundBank.SF2;
using GotaSoundIO.Sound.Formats;
using NitroFileLoader;
using NitroFileLoader.Instrument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DlsInstrument = GotaSoundBank.DLS.Instrument;
using NitroInstrument = NitroFileLoader.Instrument.Instrument;

namespace NitroStudio2.ViewModels
{
    /// <summary>
    /// Importing a DLS or SoundFont over an existing bank. Ported from
    /// MainWindow.ReplaceBankWithDLS; SoundFont import still goes through the same path by
    /// converting to DLS first.
    /// </summary>
    public sealed partial class SoundArchiveViewModel
    {
        /// <summary>Set by the host so the two picker dialogs can be shown from here.</summary>
        public Func<InstrumentSelectorViewModel, Task> ShowInstrumentSelectorRequested { get; set; }

        public Func<WaveMapperViewModel, Task> ShowWaveMapperRequested { get; set; }

        private Task ReplaceBankWithSoundFontAsync(BankInfo bank, SoundFont soundFont) =>
            ReplaceBankWithDlsAsync(bank, new DownloadableSounds(soundFont));

        private async Task ReplaceBankWithDlsAsync(BankInfo b, DownloadableSounds d)
        {
            // Offer every instrument that actually has regions, previewing its first sample.
            List<RiffWave> previewSamples = [];
            List<int> instrumentIds = [];
            List<string> instrumentNames = [];
            foreach (DlsInstrument i in d.Instruments)
            {
                if (i.Regions.Count > 0)
                {
                    instrumentIds.Add((int)(i.InstrumentId + (i.BankId * 128)));
                    instrumentNames.Add(i.Name);
                    previewSamples.Add(d.Waves[(int)i.Regions[0].WaveId]);
                }
            }

            InstrumentSelectorViewModel selector =
                new(previewSamples, instrumentIds, instrumentNames);
            await ShowSelector(selector);
            instrumentIds = selector.SelectedInstruments;
            if (instrumentIds is null)
            {
                return;
            }

            List<DlsInstrument> instruments =
            [
                .. instrumentIds.Select(id =>
                    d.Instruments.FirstOrDefault(x =>
                        x.InstrumentId == id % 128 && x.BankId == id / 128
                    )
                ),
            ];

            // Collect the distinct samples the chosen instruments need, de-duplicated by MD5.
            List<RiffWave> samples = [];
            List<string> md5s = [];
            List<WaveArchiveInfo> waveArchives = [.. b.WaveArchives.Where(x => x is not null)];
            Dictionary<uint, int> sampleIndexByWaveId = [];
            foreach (DlsInstrument instrument in instruments)
            {
                foreach (Region r in instrument.Regions)
                {
                    RiffWave wav = d.Waves[(int)r.WaveId];
                    wav.Loops = r.Loops;
                    wav.LoopStart = r.LoopStart;
                    wav.LoopEnd =
                        r.LoopLength == 0
                            ? (uint)wav.Audio.NumSamples
                            : r.LoopStart + r.LoopLength;
                    string md5 = wav.Md5Sum;
                    if (!md5s.Contains(md5))
                    {
                        samples.Add(wav);
                        md5s.Add(md5);
                        sampleIndexByWaveId.Add(r.WaveId, sampleIndexByWaveId.Count);
                    }
                    else if (!sampleIndexByWaveId.ContainsKey(r.WaveId))
                    {
                        sampleIndexByWaveId.Add(r.WaveId, md5s.IndexOf(md5));
                    }
                }
            }

            WaveMapperViewModel mapper = new(samples, waveArchives);
            await ShowMapper(mapper);
            List<ushort> warMap = mapper.WarMap;
            if (warMap is null)
            {
                return;
            }

            // Write each sample into its chosen archive and remember where it landed.
            Dictionary<int, Tuple<ushort, ushort>> swavMap = [];
            foreach (RiffWave w in samples)
            {
                Wave wav = new();
                wav.FromOtherStreamFile(w);
                WaveArchiveInfo war = SA.WaveArchives.FirstOrDefault(x =>
                    x.Index == warMap[samples.IndexOf(w)]
                );
                string md5 = wav.Md5Sum;
                if (!war.File.Waves.Any(x => x.Md5Sum == md5))
                {
                    war.File.Waves.Add(wav);
                }
                swavMap.Add(
                    samples.IndexOf(w),
                    new Tuple<ushort, ushort>(
                        (ushort)b.WaveArchives.ToList().IndexOf(war),
                        (ushort)war.File.Waves.IndexOf(war.File.Waves.First(x => x.Md5Sum == md5))
                    )
                );
            }

            b.File.Instruments = [];
            foreach (DlsInstrument instrument in instruments)
            {
                // A single region starting at note 0 is a direct instrument; up to eight is a
                // key split; anything larger becomes a drum set.
                NitroInstrument i =
                    instrument.Regions.Count < 2 && instrument.Regions.Any(x => x.NoteLow == 0)
                        ? new DirectInstrument()
                        : instrument.Regions.Count < 9
                            && instrument.Regions.Any(x => x.NoteLow == 0)
                            ? new KeySplitInstrument()
                            : new DrumSetInstrument();
                i.Index = (int)(instrument.InstrumentId + (instrument.BankId * 128));

                List<Region> regions = [.. instrument.Regions.OrderBy(x => x.NoteLow)];
                if (regions[0].NoteLow != 0 && i is DrumSetInstrument drumSet)
                {
                    drumSet.Min = (byte)regions[0].NoteLow;
                }

                foreach (Region r in regions)
                {
                    Tuple<ushort, ushort> location = swavMap[sampleIndexByWaveId[r.WaveId]];
                    NoteInfo n = new()
                    {
                        WarId = location.Item1,
                        WaveId = location.Item2,
                        InstrumentType = InstrumentType.PCM,
                        BaseNote = (byte)(r.RootNote + (r.Tuning / 65536d / 12)),
                        Key = (Notes)r.NoteHigh,
                        Attack = 127,
                        Decay = 127,
                        Sustain = 127,
                        Release = 127,
                        Pan = 64,
                    };
                    ApplyArticulators(r, n);
                    i.NoteInfo.Add(n);
                }
                b.File.Instruments.Add(i);
            }
        }

        /// <summary>Maps a DLS region's envelope and pan connections onto the note's fields.</summary>
        private static void ApplyArticulators(Region r, NoteInfo n)
        {
            foreach (Articulator a in r.Articulators)
            {
                foreach (Connection c in a.Connections)
                {
                    switch (c.DestinationConnection)
                    {
                        case DestinationConnection.EG1AttackTime when c.Scale != int.MinValue:
                            n.Attack = Bank.GetNearestTableIndex(
                                Bank.TimecentsToMilliseconds(c.Scale / 65536),
                                Bank.AttackTable
                            );
                            break;
                        case DestinationConnection.EG1DecayTime when c.Scale != int.MinValue:
                            n.Decay = Bank.GetNearestTableIndex(
                                Bank.TimecentsToMilliseconds(c.Scale / 65536),
                                Bank.MaxReleaseTimes
                            );
                            break;
                        case DestinationConnection.EG1SustainLevel:
                            n.Sustain = Bank.Fraction2Sustain(c.Scale / 65536 / 1000d);
                            break;
                        case DestinationConnection.EG1ReleaseTime when c.Scale != int.MinValue:
                            n.Release = Bank.GetNearestTableIndex(
                                Bank.TimecentsToMilliseconds(c.Scale / 65536),
                                Bank.MaxReleaseTimes
                            );
                            break;
                        case DestinationConnection.Pan:
                            n.Pan = Bank.SetPan(c.Scale / 65536);
                            break;
                    }
                }
            }
        }

        private Task ShowSelector(InstrumentSelectorViewModel vm) =>
            ShowInstrumentSelectorRequested?.Invoke(vm) ?? Task.CompletedTask;

        private Task ShowMapper(WaveMapperViewModel vm) =>
            ShowWaveMapperRequested?.Invoke(vm) ?? Task.CompletedTask;
    }
}
