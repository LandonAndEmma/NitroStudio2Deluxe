using GotaSequenceLib.Playback;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages;
using Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Sequencing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Track = Sanford.Multimedia.Midi.Track;

namespace GotaSequenceLib
{
    public static class SMF
    {
        private static readonly Random _rand = new();

        public static Sequence FromSequenceCommands(
            List<SequenceCommand> commands,
            int startIndex,
            ushort trackMask = 0xFFFF
        )
        {
            Sequence m = new(960) { Format = 1 };
            m.Add(new Track());
            Dictionary<int, int> tickMap = [];
            short[] vars = new short[]
            {
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
            };
            WriteTrack(m, tickMap, commands, 0, m[0], m[0], startIndex, ref vars, 0, trackMask);
            int labelNum = 0;
            List<long> jumpsAdded = [];
            List<Tuple<int, int>> jumpsToAdd = [];
            foreach (
                KeyValuePair<int, int> cmdTicks in tickMap.Where(x =>
                    Player.GetTrueCommandType(commands[x.Key]) == SequenceCommands.Jump
                )
            )
            {
                int ticksJump = cmdTicks.Value;
                SequenceCommand cmd = commands[cmdTicks.Key];
                int arg = Player.GetCommandParameter(cmd, 0, _rand, commands);
                int ticks = tickMap[arg];
                long tickHash = (ticks << 32) | ticksJump;
                if (!jumpsAdded.Contains(tickHash))
                {
                    jumpsAdded.Add(tickHash);
                    jumpsToAdd.Add(new Tuple<int, int>(ticks, ticksJump));
                }
            }
            foreach (Tuple<int, int> j in jumpsToAdd)
            {
                m[0]
                    .Insert(
                        j.Item1,
                        new MetaMessage(
                            MetaType.Marker,
                            Encoding.UTF8.GetBytes(
                                jumpsToAdd.Count == 1 ? "[" : ("Label_" + labelNum)
                            )
                        )
                    );
                m[0]
                    .Insert(
                        j.Item2,
                        new MetaMessage(
                            MetaType.Marker,
                            Encoding.UTF8.GetBytes(
                                jumpsToAdd.Count == 1 ? "]" : ("jump Label_" + labelNum)
                            )
                        )
                    );
                labelNum++;
            }
            return m;
        }

        public static void WriteTrack(
            Sequence m,
            Dictionary<int, int> tickMap,
            List<SequenceCommand> commands,
            int trackNum,
            Track t,
            Track metaTrack,
            int startIndex,
            ref short[] vars,
            int startTicks = 0,
            ushort trackMask = 0xFFFF
        )
        {
            int currCommand = startIndex;
            int ticks = startTicks;
            bool noteWait = true;
            int[] callStack = new int[3];
            int callStackDepth = 0;
            bool tie = false;
            bool varFlag = false;
            short[] trackVars = new short[]
            {
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
            };
            int timeBase = 48;
            int trackIndex = trackNum;
            short[] Vars = vars;
            short GetVar(int varNum, int h)
            {
                return varNum < 0x20 ? Vars[varNum] : trackVars[varNum - 0x20];
            }
            void SetVar(int varNum, int h, short val)
            {
                if (varNum < 0x20)
                {
                    Vars[varNum] = val;
                }
                else
                {
                    trackVars[varNum - 0x20] = val;
                }
            }
            while (currCommand < commands.Count)
            {
                SequenceCommand c = commands[currCommand];
                if (!tickMap.ContainsKey(currCommand))
                {
                    tickMap.Add(currCommand, ticks);
                }
                int numArgs = Player.NumArguments(c);
                int[] args = new int[numArgs];
                for (int i = 0; i < numArgs; i++)
                {
                    args[i] = Player.GetCommandParameter(c, i, _rand, commands);
                }
                SequenceCommands trueCommandType = Player.GetTrueCommandType(c);
                if ((trackMask & 0b1) == 0)
                {
                    if (trueCommandType == SequenceCommands.OpenTrack)
                    {
                        if (((0b1 << args[0]) & trackMask) > 0)
                        {
                            while (m.Count - 1 < args[0])
                            {
                                m.Add(new Track());
                            }
                            WriteTrack(
                                m,
                                tickMap,
                                commands,
                                args[0],
                                m[args[0]],
                                metaTrack,
                                args[1],
                                ref vars,
                                ticks
                            );
                        }
                    }
                    currCommand++;
                    continue;
                }
                if (c.CommandType == SequenceCommands.If && !varFlag)
                {
                    currCommand++;
                    continue;
                }
                switch (trueCommandType)
                {
                    case SequenceCommands.AllocateTrack:
                        break;
                    case SequenceCommands.Note:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.NoteOn, trackNum, args[0], args[1])
                        );
                        if (!tie)
                        {
                            t.Insert(
                                ticks + Sequence2MidiTicks(args[2], 960, timeBase),
                                new ChannelMessage(ChannelCommand.NoteOff, trackNum, args[0])
                            );
                        }

                        _ = args[0];
                        if (noteWait)
                        {
                            ticks += Sequence2MidiTicks(args[2], 960, timeBase);
                        }
                        break;
                    case SequenceCommands.Wait:
                        ticks += Sequence2MidiTicks(args[0], 960, timeBase);
                        break;
                    case SequenceCommands.ProgramChange:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.ProgramChange, trackNum, args[0])
                        );
                        break;
                    case SequenceCommands.OpenTrack:
                        if (((0b1 << args[0]) & trackMask) > 0)
                        {
                            while (m.Count - 1 < args[0])
                            {
                                m.Add(new Track());
                            }
                            WriteTrack(
                                m,
                                tickMap,
                                commands,
                                args[0],
                                m[args[0]],
                                metaTrack,
                                args[1],
                                ref vars,
                                ticks
                            );
                        }
                        break;
                    case SequenceCommands.Jump:
                        break;
                    case SequenceCommands.Call:
                        if (callStackDepth < 3)
                        {
                            callStack[callStackDepth] = currCommand + 1;
                            callStackDepth++;
                            currCommand = args[0];
                            continue;
                        }
                        break;
                    case SequenceCommands.Timebase:
                        timeBase = args[0];
                        break;
                    case SequenceCommands.EnvHold:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 79, args[0])
                        );
                        break;
                    case SequenceCommands.Monophonic:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                68,
                                args[0] >= 0 ? 0x7F : 0
                            )
                        );
                        break;
                    case SequenceCommands.BiquadType:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 30, args[0])
                        );
                        break;
                    case SequenceCommands.BiquadValue:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 31, args[0])
                        );
                        break;
                    case SequenceCommands.BankSelect:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.BankSelect,
                                args[0]
                            )
                        );
                        break;
                    case SequenceCommands.Pan:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.Pan,
                                args[0]
                            )
                        );
                        break;
                    case SequenceCommands.Volume:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.Volume,
                                args[0]
                            )
                        );
                        break;
                    case SequenceCommands.MainVolume:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 12, args[0])
                        );
                        break;
                    case SequenceCommands.Transpose:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                13,
                                args[0] + 0x40
                            )
                        );
                        break;
                    case SequenceCommands.PitchBend:
                        Tuple<int, int> pitch = PitchBend2Midi(args[0] / 127d);
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.PitchWheel,
                                trackNum,
                                pitch.Item2,
                                pitch.Item1
                            )
                        );
                        break;
                    case SequenceCommands.BendRange:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.RegisteredParameterCoarse,
                                0
                            )
                        );
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.RegisteredParameterFine,
                                0
                            )
                        );
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 6, args[0])
                        );
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.RegisteredParameterCoarse,
                                127
                            )
                        );
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.RegisteredParameterFine,
                                127
                            )
                        );
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 20, args[0])
                        );
                        break;
                    case SequenceCommands.Prio:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 14, args[0])
                        );
                        break;
                    case SequenceCommands.NoteWait:
                        noteWait = args[0] > 0;
                        break;
                    case SequenceCommands.Tie:
                        tie = args[0] > 0;
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.AllNotesOff
                            )
                        );
                        break;
                    case SequenceCommands.Porta:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 84, args[0])
                        );
                        break;
                    case SequenceCommands.ModDepth:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 1, args[0])
                        );
                        break;
                    case SequenceCommands.ModSpeed:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 21, args[0])
                        );
                        break;
                    case SequenceCommands.ModType:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 22, args[0])
                        );
                        break;
                    case SequenceCommands.ModRange:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 23, args[0])
                        );
                        break;
                    case SequenceCommands.PortaSw:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.Portamento,
                                args[0] > 0 ? 0x7F : 0
                            )
                        );
                        break;
                    case SequenceCommands.PortaTime:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                (int)ControllerType.PortamentoTime,
                                args[0]
                            )
                        );
                        break;
                    case SequenceCommands.Attack:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 85, args[0])
                        );
                        break;
                    case SequenceCommands.Decay:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 86, args[0])
                        );
                        break;
                    case SequenceCommands.Sustain:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 87, args[0])
                        );
                        break;
                    case SequenceCommands.Release:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 88, args[0])
                        );
                        break;
                    case SequenceCommands.LoopStart:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 89, args[0])
                        );
                        break;
                    case SequenceCommands.Volume2:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 11, args[0])
                        );
                        break;
                    case SequenceCommands.FxSendA:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 91, args[0])
                        );
                        break;
                    case SequenceCommands.FxSendB:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 92, args[0])
                        );
                        break;
                    case SequenceCommands.MainSend:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 95, args[0])
                        );
                        break;
                    case SequenceCommands.SurroundPan:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 9, args[0])
                        );
                        break;
                    case SequenceCommands.InitPan:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 3, args[0])
                        );
                        break;
                    case SequenceCommands.FxSendC:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 93, args[0])
                        );
                        break;
                    case SequenceCommands.Damper:
                        t.Insert(
                            ticks,
                            new ChannelMessage(
                                ChannelCommand.Controller,
                                trackNum,
                                64,
                                args[0] >= 0 ? 0x7F : 0
                            )
                        );
                        break;
                    case SequenceCommands.ModDelay:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 26, args[0])
                        );
                        break;
                    case SequenceCommands.Tempo:
                        TempoChangeBuilder change = new() { Tempo = 60000000 / args[0] };
                        change.Build();
                        metaTrack.Insert(ticks, change.Result);
                        break;
                    case SequenceCommands.LoopEnd:
                        t.Insert(
                            ticks,
                            new ChannelMessage(ChannelCommand.Controller, trackNum, 90)
                        );
                        break;
                    case SequenceCommands.Return:
                        if (callStackDepth != 0)
                        {
                            callStackDepth--;
                            currCommand = callStack[callStackDepth];
                            continue;
                        }
                        break;
                    case SequenceCommands.Fin:
                        return;
                    case SequenceCommands.SetVar:
                        switch (args[0])
                        {
                            case 0:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        16,
                                        args[1]
                                    )
                                );
                                break;
                            case 1:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        17,
                                        args[1]
                                    )
                                );
                                break;
                            case 2:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        18,
                                        args[1]
                                    )
                                );
                                break;
                            case 3:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        19,
                                        args[1]
                                    )
                                );
                                break;
                            case 32:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        80,
                                        args[1]
                                    )
                                );
                                break;
                            case 33:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        81,
                                        args[1]
                                    )
                                );
                                break;
                            case 34:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        82,
                                        args[1]
                                    )
                                );
                                break;
                            case 35:
                                t.Insert(
                                    ticks,
                                    new ChannelMessage(
                                        ChannelCommand.Controller,
                                        trackNum,
                                        83,
                                        args[1]
                                    )
                                );
                                break;
                            default:
                                metaTrack.Insert(
                                    ticks,
                                    new MetaMessage(
                                        MetaType.Marker,
                                        Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                                    )
                                );
                                break;
                        }
                        break;
                    case SequenceCommands.AddVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) + args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.SubVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) - args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.MulVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) * args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.DivVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) / args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.ShiftVar:
                        SetVar(
                            args[0],
                            trackIndex,
                            args[1] < 0
                                ? (short)(GetVar(args[0], trackIndex) >> -args[1])
                                : (short)(GetVar(args[0], trackIndex) << args[1])
                        );
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.RandVar:
                        {
                            bool negate = false;
                            if (args[1] < 0)
                            {
                                negate = true;
                                args[1] = (short)-args[1];
                            }
                            short val = (short)_rand.Next(args[1] + 1);
                            if (negate)
                            {
                                val = (short)-val;
                            }
                            SetVar(args[0], trackIndex, val);
                            metaTrack.Insert(
                                ticks,
                                new MetaMessage(
                                    MetaType.Marker,
                                    Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                                )
                            );
                            break;
                        }
                    case SequenceCommands.AndVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) & args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.OrVar:
                        SetVar(
                            args[0],
                            trackIndex,
                            (short)(GetVar(args[0], trackIndex) | (short)args[1])
                        );
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.XorVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) ^ args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.NotVar:
                        SetVar(
                            args[0],
                            trackIndex,
                            (short)(
                                (~(GetVar(args[0], trackIndex) & args[1]))
                                | (GetVar(args[0], trackIndex) & (~args[0]))
                            )
                        );
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.ModVar:
                        SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) % args[1]));
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.CmpEq:
                        varFlag = GetVar(args[0], trackIndex) == args[1];
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.CmpGe:
                        varFlag = GetVar(args[0], trackIndex) >= args[1];
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.CmpGt:
                        varFlag = GetVar(args[0], trackIndex) > args[1];
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.CmpLe:
                        varFlag = GetVar(args[0], trackIndex) <= args[1];
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.CmpLt:
                        varFlag = GetVar(args[0], trackIndex) < args[1];
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    case SequenceCommands.CmpNe:
                        varFlag = GetVar(args[0], trackIndex) != args[1];
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                    default:
                        metaTrack.Insert(
                            ticks,
                            new MetaMessage(
                                MetaType.Marker,
                                Encoding.UTF8.GetBytes(trackNum + ": " + c.ToString())
                            )
                        );
                        break;
                }
                currCommand++;
            }
        }

        public static List<SequenceCommand> ToSequenceCommands(
            Sequence s,
            out Dictionary<string, int> labels,
            out List<int> privateLabels,
            string sequenceName,
            int timeBase = 48,
            bool privateLabelsForCalls = false
        )
        {
            List<SequenceCommand> commands = [];
            labels = [];
            privateLabels = [];
            labels.Add("SMF_" + sequenceName + "_Begin", 0);
            List<int> allocs = [];
            if (s.Count > 1)
            {
                ushort alloc = 0;
                for (int i = 0; i < s.Count; i++)
                {
                    alloc |= (ushort)(0b1 << i);
                    allocs.Add(i);
                }
                commands.Add(
                    new SequenceCommand()
                    {
                        CommandType = SequenceCommands.AllocateTrack,
                        Parameter = alloc,
                    }
                );
            }
            int openTrackOff = commands.Count;
            for (int i = 1; i < s.Count; i++)
            {
                commands.Add(
                    new SequenceCommand()
                    {
                        CommandType = SequenceCommands.OpenTrack,
                        Parameter = new OpenTrackParameter() { TrackNumber = (byte)i },
                    }
                );
            }
            Dictionary<string, int> otherLabelTicks = [];
            int loopStartTicks = -1;
            int loopEndTicks = -1;
            labels.Add("SMF_" + sequenceName + "_Start", 1);
            for (int i = 0; i < allocs.Count; i++)
            {
                labels.Add("SMF_" + sequenceName + "_Track_" + allocs[i], commands.Count);
                if (i != 0)
                {
                    (commands[1 + i - 1].Parameter as OpenTrackParameter).m_Index = commands.Count;
                }
                ReadTrack(
                    commands,
                    s,
                    allocs[i],
                    openTrackOff,
                    labels,
                    timeBase,
                    sequenceName,
                    otherLabelTicks,
                    ref loopStartTicks,
                    ref loopEndTicks
                );
            }
            labels.Add("SMF_" + sequenceName + "_End", commands.Count);
            commands.Add(new SequenceCommand() { CommandType = SequenceCommands.Fin });
            return commands;
        }

        public static void ReadTrack(
            List<SequenceCommand> commands,
            Sequence s,
            int trackNum,
            int openTrackOffset,
            Dictionary<string, int> labels,
            int timeBase,
            string sequenceName,
            Dictionary<string, int> otherLabelTicks,
            ref int loopStartTicks,
            ref int loopEndTicks
        )
        {
            int startTrackPointer = commands.Count;
            List<MidiEvent> events = [];
            for (int i = 0; i < s[trackNum].Count; i++)
            {
                events.Add(s[trackNum].GetMidiEvent(i));
            }
            events = events.OrderBy(x => x.AbsoluteTicks).ToList();
            commands.Add(
                new SequenceCommand() { CommandType = SequenceCommands.NoteWait, Parameter = false }
            );
            int eventNum = 0;
            int lastTick = 0;
            foreach (MidiEvent e in events)
            {
                uint overtime = 0;
                switch (e.MidiMessage.MessageType)
                {
                    case MessageType.Channel:
                        ChannelMessage con = e.MidiMessage as ChannelMessage;
                        switch (con.Command)
                        {
                            case ChannelCommand.NoteOn:
                                int len = 0;
                                int key = con.Data1;
                                AddWaitTime();
                                for (int i = eventNum + 1; i < events.Count; i++)
                                {
                                    if (
                                        (events[i].MidiMessage as ChannelMessage) != null
                                        && (events[i].MidiMessage as ChannelMessage).Command
                                            == ChannelCommand.NoteOff
                                        && (events[i].MidiMessage as ChannelMessage).Data1 == key
                                    )
                                    {
                                        len = Midi2SequenceTicks(
                                            events[i].AbsoluteTicks - e.AbsoluteTicks,
                                            s.Division,
                                            timeBase
                                        );
                                        break;
                                    }
                                }
                                commands.Add(
                                    new SequenceCommand()
                                    {
                                        CommandType = SequenceCommands.Note,
                                        Parameter = new NoteParameter()
                                        {
                                            Note = (Notes)key,
                                            Velocity = (byte)con.Data2,
                                            Length = (uint)len,
                                        },
                                    }
                                );
                                overtime = (uint)len;
                                break;
                            case ChannelCommand.ProgramChange:
                                AddWaitTime();
                                commands.Add(
                                    new SequenceCommand()
                                    {
                                        CommandType = SequenceCommands.ProgramChange,
                                        Parameter = (uint)con.Data1,
                                    }
                                );
                                break;
                            case ChannelCommand.PitchWheel:
                                AddWaitTime();
                                commands.Add(
                                    new SequenceCommand()
                                    {
                                        CommandType = SequenceCommands.PitchBend,
                                        Parameter = (sbyte)(
                                            Midi2PitchBend(con.Data2, con.Data1) * 127
                                        ),
                                    }
                                );
                                break;
                            case ChannelCommand.Controller:
                                switch ((ControllerType)con.Data1)
                                {
                                    case ControllerType.BankSelect:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.BankSelect,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)1:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.ModDepth,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)3:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.InitPan,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case ControllerType.PortamentoTime:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.PortaTime,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case ControllerType.Volume:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Volume,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)9:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SurroundPan,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case ControllerType.Pan:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Pan,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)11:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Volume2,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)12:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.MainVolume,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)13:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Transpose,
                                                Parameter = (sbyte)(con.Data2 - 0x40),
                                            }
                                        );
                                        break;
                                    case (ControllerType)14:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Prio,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)16:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 0,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)17:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 1,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)18:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 2,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)19:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 3,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)20:
                                        AddWaitTime();
                                        if (
                                            commands.Last().CommandType
                                            == SequenceCommands.BendRange
                                        )
                                        {
                                            break;
                                        }
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.BendRange,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)21:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.ModSpeed,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)22:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.ModType,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)23:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.ModRange,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)26:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.ModDelay,
                                                Parameter = (short)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)27:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.ModDelay,
                                                Parameter = (short)(con.Data2 * 10),
                                            }
                                        );
                                        break;
                                    case (ControllerType)28:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SweepPitch,
                                                Parameter = (short)(con.Data2 - 0x40),
                                            }
                                        );
                                        break;
                                    case (ControllerType)29:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SweepPitch,
                                                Parameter = (short)((con.Data2 - 0x40) * 24),
                                            }
                                        );
                                        break;
                                    case (ControllerType)30:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.BiquadType,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)31:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.BiquadValue,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)64:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Damper,
                                                Parameter = con.Data2 >= 64,
                                            }
                                        );
                                        break;
                                    case ControllerType.Portamento:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.PortaSw,
                                                Parameter = con.Data2 >= 64,
                                            }
                                        );
                                        break;
                                    case ControllerType.LegatoPedal:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Monophonic,
                                                Parameter = con.Data2 >= 64,
                                            }
                                        );
                                        break;
                                    case (ControllerType)79:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.EnvHold,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)80:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 32,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)81:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 33,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)82:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 34,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)83:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.SetVar,
                                                Parameter = new U8S16Parameter()
                                                {
                                                    U8 = 35,
                                                    S16 = (short)con.Data2,
                                                },
                                            }
                                        );
                                        break;
                                    case (ControllerType)84:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Porta,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)85:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Attack,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)86:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Decay,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)87:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Sustain,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)88:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.Release,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)89:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.LoopStart,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)90:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.LoopEnd,
                                            }
                                        );
                                        break;
                                    case (ControllerType)91:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.FxSendA,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)92:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.FxSendB,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)93:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.FxSendC,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case (ControllerType)95:
                                        AddWaitTime();
                                        commands.Add(
                                            new SequenceCommand()
                                            {
                                                CommandType = SequenceCommands.MainSend,
                                                Parameter = (byte)con.Data2,
                                            }
                                        );
                                        break;
                                    case ControllerType.RegisteredParameterCoarse:
                                        if (con.Data2 == 0 && (RPNFine0(1) || RPNFine0(-1)))
                                        {
                                            if (RPNCourse(1))
                                            {
                                                AddWaitTime();
                                                if (
                                                    commands.Last().CommandType
                                                    == SequenceCommands.BendRange
                                                )
                                                {
                                                    break;
                                                }
                                                commands.Add(
                                                    new SequenceCommand()
                                                    {
                                                        CommandType = SequenceCommands.BendRange,
                                                        Parameter = (byte)
                                                            (
                                                                events[eventNum + 1].MidiMessage
                                                                as ChannelMessage
                                                            ).Data2,
                                                    }
                                                );
                                            }
                                            else if (RPNCourse(2))
                                            {
                                                AddWaitTime();
                                                if (
                                                    commands.Last().CommandType
                                                    == SequenceCommands.BendRange
                                                )
                                                {
                                                    break;
                                                }
                                                commands.Add(
                                                    new SequenceCommand()
                                                    {
                                                        CommandType = SequenceCommands.BendRange,
                                                        Parameter = (byte)
                                                            (
                                                                events[eventNum + 2].MidiMessage
                                                                as ChannelMessage
                                                            ).Data2,
                                                    }
                                                );
                                            }
                                        }
                                        bool RPNFine0(int eventOff)
                                        {
                                            int off = eventNum + eventOff;
                                            if (off < 0 || off >= events.Count)
                                            {
                                                return false;
                                            }
                                            if ((events[off].MidiMessage as ChannelMessage) != null)
                                            {
                                                if (
                                                    (
                                                        events[off].MidiMessage as ChannelMessage
                                                    ).Command == ChannelCommand.Controller
                                                    && (
                                                        events[off].MidiMessage as ChannelMessage
                                                    ).Data1
                                                        == (int)
                                                            ControllerType.RegisteredParameterFine
                                                    && (
                                                        events[off].MidiMessage as ChannelMessage
                                                    ).Data2 == 0
                                                )
                                                {
                                                    return true;
                                                }
                                            }
                                            return false;
                                        }
                                        bool RPNCourse(int eventOff)
                                        {
                                            int off = eventNum + eventOff;
                                            if (off < 0 || off >= events.Count)
                                            {
                                                return false;
                                            }
                                            if ((events[off].MidiMessage as ChannelMessage) != null)
                                            {
                                                if (
                                                    (
                                                        events[off].MidiMessage as ChannelMessage
                                                    ).Command == ChannelCommand.Controller
                                                    && (
                                                        events[off].MidiMessage as ChannelMessage
                                                    ).Data1 == 6
                                                )
                                                {
                                                    return true;
                                                }
                                            }
                                            return false;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case MessageType.Meta:
                        MetaMessage met = e.MidiMessage as MetaMessage;
                        switch (met.MetaType)
                        {
                            case MetaType.Tempo:
                                AddWaitTime();
                                byte[] tempoRaw = met.GetBytes();
                                uint tempoVal = (uint)(
                                    (tempoRaw[0] << 16) | (tempoRaw[1] << 8) | tempoRaw[2]
                                );
                                commands.Add(
                                    new SequenceCommand()
                                    {
                                        CommandType = SequenceCommands.Tempo,
                                        Parameter = (short)(60000000 / tempoVal),
                                    }
                                );
                                break;
                            case MetaType.CuePoint:
                            case MetaType.Marker:
                                AddWaitTime();
                                string dat = Encoding.UTF8.GetString(met.GetBytes());
                                if (dat.Contains(": "))
                                {
                                    try
                                    {
                                        SequenceCommand c = new();
                                        if (int.Parse(dat.Split(':')[0]) == trackNum)
                                        {
                                            c.FromString(
                                                dat[(dat.IndexOf(":") + 2)..],
                                                labels,
                                                []
                                            );
                                            commands.Add(c);
                                        }
                                    }
                                    catch { }
                                }
                                else
                                {
                                    string loopStartStr =
                                        "SMF_"
                                        + sequenceName
                                        + "_Track_"
                                        + trackNum
                                        + "_SSN_LOOPSTART";
                                    string loopEndStr =
                                        "SMF_"
                                        + sequenceName
                                        + "_Track_"
                                        + trackNum
                                        + "_SSN_LOOPEND";
                                    if (
                                        !labels.ContainsKey(loopStartStr)
                                        && (
                                            dat.Equals("[")
                                            || dat.ToLower().Equals("loopstart")
                                            || dat.ToLower().Equals("loop_start")
                                        )
                                    )
                                    {
                                        labels.Add(loopStartStr, commands.Count);
                                        loopStartTicks = e.AbsoluteTicks;
                                    }
                                    else if (
                                        !labels.ContainsKey(loopEndStr)
                                        && (
                                            dat.Equals("]")
                                            || dat.ToLower().Equals("loopend")
                                            || dat.ToLower().Equals("loop_end")
                                        )
                                    )
                                    {
                                        loopEndTicks = e.AbsoluteTicks;
                                    }
                                    else
                                    {
                                        labels.Add(dat, commands.Count);
                                    }
                                }
                                break;
                        }
                        break;
                }
                eventNum++;
                if (eventNum == events.Count && overtime != 0)
                {
                    commands.Add(
                        new SequenceCommand()
                        {
                            CommandType = SequenceCommands.Wait,
                            Parameter = overtime,
                        }
                    );
                }
                void AddWaitTime()
                {
                    int waitTime = e.AbsoluteTicks - lastTick;
                    if (waitTime != 0)
                    {
                        commands.Add(
                            new SequenceCommand()
                            {
                                CommandType = SequenceCommands.Wait,
                                Parameter = (uint)Midi2SequenceTicks(
                                    waitTime,
                                    s.Division,
                                    timeBase
                                ),
                            }
                        );
                    }
                    lastTick = e.AbsoluteTicks;
                }
            }
            commands.Add(new SequenceCommand() { CommandType = SequenceCommands.Fin });
            if (trackNum != 0)
            {
                (
                    commands[openTrackOffset + trackNum - 1].Parameter as OpenTrackParameter
                ).ReferenceCommand = commands[startTrackPointer];
            }
        }

        public static Tuple<int, int> PitchBend2Midi(double pitchAmount)
        {
            ushort zeroPitch = 0x2000;
            ushort pitch = (ushort)(zeroPitch + (pitchAmount * 0x2000));
            if (pitch > 0x3FFF)
            {
                pitch = 0x3FFF;
            }
            int msb = (pitch & 0x3F80) >> 7;
            int lsb = pitch & 0x7F;
            return new Tuple<int, int>(msb, lsb);
        }

        public static Tuple<int, int> Scale2Midi(int val)
        {
            ushort v = (ushort)(val / 127d * 0x3FFF);
            int msb = (v & 0x3F80) >> 7;
            int lsb = v & 0x7F;
            return new Tuple<int, int>(msb, lsb);
        }

        public static int Midi2SequenceTicks(int midiTicks, int division, int timeBase = 48)
        {
            return (int)(midiTicks / (double)division * timeBase);
        }

        public static int Sequence2MidiTicks(int sequenceTicks, int division, int timeBase = 48)
        {
            return (int)(sequenceTicks * division / (double)timeBase);
        }

        public static double Midi2PitchBend(int msb, int lsb)
        {
            ushort val = (ushort)(msb << 7);
            val |= (ushort)(lsb & 0x7F);
            if (val > 0x3FFF)
            {
                val = 0x3FFF;
            }
            return (val - 0x2000) / (double)0x2000;
        }
    }
}
