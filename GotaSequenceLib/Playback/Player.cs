using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
namespace GotaSequenceLib.Playback {
    public class Player : IDisposable {
        public uint ClockSpeed = 16756991;
        public short[] Vars = new short[0x20];
        public PlayableBank[] Banks;
        public RiffWave[][] WaveArchives;
        public byte Volume = 127;
        private int Timebase {
            get => _ticksPerWholeNote / 4;
            set {
                _ticksPerWholeNote = value * 4;
                _time = new TimeBarrier(_ticksPerWholeNote);
            }
        }
        private readonly Track[] _tracks = new Track[0x10];
        private readonly Mixer _mixer;
        private TimeBarrier _time;
        private Thread _thread;
        private int _randSeed;
        private Random _rand;
        private ushort _tempo;
        private int _tempoStack;
        private long _elapsedLoops;
        private int _ticksPerWholeNote = 192;
        private int currEventOverride;
        public List<SequenceCommand> Events { get; private set; }
        public Dictionary<int, int> Ticks { get; private set; }
        public long ElapsedTicks { get; private set; }
        public long MaxTicks { get; private set; }
        public bool ShouldFadeOut { get; set; } = true;
        public bool DontFadeSong { get; set; }
        public long NumLoops { get; set; } = 0;
        private int _longestTrack;
        public PlayerState State { get; private set; }
        public event SongEndedEvent SongEnded;
        public event NotePressedHandler NotePressed = delegate {};
        public event NotePressedHandler NoteReleased = delegate {};
        public delegate void NotePressedHandler(object sender, NoteEventArgs e);
        public class NoteEventArgs : EventArgs {
            public int TrackId;
            public Notes Note;
            public bool On;
        }
        public Player(Mixer mixer) {
            for (byte i = 0; i < 0x10; i++) {
                _tracks[i] = new Track(i, this);
            }
            _mixer = mixer;
            Timebase = 48;
        }
        public void PrepareForSong(PlayableBank[] banks, RiffWave[][] waveArchives) {
            Banks = banks;
            WaveArchives = waveArchives;
        }
        public void LoadSong(List<SequenceCommand> commands, int startOffset = 0) {
            Stop();
            Events = commands;
            _randSeed = new Random().Next();
            currEventOverride = startOffset;
            InitEmulation();
            SetTicks();
            currEventOverride = startOffset;
        }
        private void CreateThread() {
            _thread = new Thread(Tick);
            _thread.Start();
        }
        private void WaitThread() {
            if (_thread != null && (_thread.ThreadState == ThreadState.Running || _thread.ThreadState == ThreadState.WaitSleepJoin)) {
                _thread.Join();
            }
        }
        private void InitEmulation() {
            _tempo = 120; 
            _tempoStack = 0;
            _elapsedLoops = 0;
            ElapsedTicks = 0;
            Timebase = 48;
            _mixer.ResetFade();
            _rand = new Random(_randSeed);
            for (int i = 0; i < 0x10; i++) {
                _tracks[i].Init();
            }
            for (int i = 0; i < 0x10; i++) {
                Vars[i] = -1;
            }
        }
        public void PlayNote(Track track, byte key, byte velocity, int duration) {
            Channel channel = null;
            NotePressed(this, new NoteEventArgs() { TrackId = _tracks.ToList().IndexOf(track), Note = (Notes)key, On = true });
            track.NoteDown = true;
            if (track.Tie && track.Channels.Count != 0) {
                channel = track.Channels.Last();
                channel.Key = key;
                channel.NoteVelocity = velocity;
            } else {
                NotePlayBackInfo param = Banks[track.BankNum].GetNotePlayBackInfo(track.Voice, (Notes)key, velocity);
                if (param != null) {
                    InstrumentType type = param.InstrumentType;
                    channel = _mixer.AllocateChannel(type, track);
                    if (channel != null) {
                        if (track.Tie) {
                            duration = -1;
                        }
                        byte release = param.Release;
                        if (release == 0xFF) {
                            duration = -1;
                            release = 0;
                        }
                        bool started = false;
                        switch (type) {
                            case InstrumentType.PCM: {
                                RiffWave wave = null;
                                try { wave = WaveArchives[param.WarId][param.WaveId]; } catch { Console.WriteLine("Can't find wave specified by bank!"); }
                                if (wave != null) {
                                    channel.StartPCM(wave, duration, ClockSpeed);
                                    started = true;
                                }
                                break;
                            }
                            case InstrumentType.PSG: {
                                channel.StartPSG((byte)param.WaveId, duration);
                                started = true;
                                break;
                            }
                            case InstrumentType.Noise: {
                                channel.StartNoise(duration);
                                started = true;
                                break;
                            }
                        }
                        channel.Stop();
                        if (started) {
                            channel.Key = key;
                            byte baseKey = param.BaseKey;
                            channel.BaseKey = type != InstrumentType.PCM && baseKey == 0x7F ? (byte)60 : baseKey;
                            channel.NoteVelocity = velocity;
                            channel.SetAttack(param.Attack);
                            channel.SetDecay(param.Decay);
                            channel.SetSustain(param.Sustain);
                            channel.SetHold(param.Hold);
                            channel.SetRelease(release);
                            channel.StartingPan = (sbyte)(param.Pan - 0x40);
                            channel.Owner = track;
                            track.Channels.Add(channel);
                        } else {
                            return;
                        }
                    }
                }
            }
            if (channel != null) {
                if (track.Attack != 0xFF) {
                    channel.SetAttack(track.Attack);
                }
                if (track.Decay != 0xFF) {
                    channel.SetDecay(track.Decay);
                }
                if (track.Sustain != 0xFF) {
                    channel.SetSustain(track.Sustain);
                }
                if (track.Hold != 0xFF) {
                    channel.SetHold(track.Hold);
                }
                if (track.Release != 0xFF) {
                    channel.SetRelease(track.Release);
                }
                channel.SweepPitch = track.SweepPitch;
                if (track.Portamento) {
                    channel.SweepPitch += (short)((track.PortamentoKey - key) << 6); 
                }
                if (track.PortamentoTime != 0) {
                    channel.SweepLength = (track.PortamentoTime * track.PortamentoTime * Math.Abs(channel.SweepPitch)) >> 11; 
                    channel.AutoSweep = true;
                } else {
                    channel.SweepLength = duration;
                    channel.AutoSweep = false;
                }
                channel.SweepCounter = 0;
            }
        }
        public short GetVar(int varNum, int trackNum) {
            if (varNum < 0x20) {
                return Vars[varNum];
            } else {
                return _tracks[trackNum].Vars[varNum - 0x20];
            }
        }
        public void SetVar(int varNum, int trackNum, short val) {
            if (varNum < 0x20) {
                Vars[varNum] = val;
            } else {
                _tracks[trackNum].Vars[varNum - 0x20] = val;
            }
        }
        private void Tick() {
            _time.Start();
            while (true) {
                PlayerState state = State;
                bool playing = state == PlayerState.Playing;
                bool recording = state == PlayerState.Recording;
                if (!playing && !recording) {
                    goto stop;
                }
                void MixerProcess() {
                    _mixer.ChannelTick();
                    _mixer.Process(playing, recording);
                }
                while (_tempoStack >= 240) {
                    _tempoStack -= 240;
                    bool allDone = true;
                    for (int i = 0; i < 0x10; i++) {
                        Track track = _tracks[i];
                        if (track.Enabled) {
                            track.Tick();
                            if (track.NoteDown && (track.Channels.Count == 0 || track.Channels.Last().State == EnvelopeState.Release)) {
                                track.NoteDown = false;
                                NoteReleased(this, new NoteEventArgs() { On = false, TrackId = i });
                            }
                            while (track.Rest == 0 && !track.WaitingForNoteToFinishBeforeContinuingXD && !track.Stopped) {
                                ExecuteNext(i);
                            }
                            if (i == _longestTrack) {
                                if (ElapsedTicks >= MaxTicks) {
                                    if (!track.Stopped) {
                                        long[] t = Events[track.CurEvent].Ticks;
                                        ElapsedTicks = t.Length == 0 ? 0 : t[_longestTrack] - track.Rest; 
                                        _elapsedLoops++;
                                        if (ShouldFadeOut && !_mixer.IsFading() && _elapsedLoops > NumLoops) {
                                            _mixer.BeginFadeOut();
                                        }
                                    }
                                } else {
                                    ElapsedTicks++;
                                }
                            }
                            if (!track.Stopped || track.Channels.Count != 0) {
                                allDone = false;
                            }
                        }
                    }
                    if (_mixer.IsFadeDone()) {
                        allDone = true;
                    }
                    if (allDone) {
                        MixerProcess();
                        State = PlayerState.Stopped;
                        SongEnded?.Invoke();
                        goto stop;
                    }
                }
                _tempoStack += _tempo;
                MixerProcess();
                if (playing) {
                    _time.Wait();
                }
            }
        stop:
            _time.Stop();
        }
        public static int GetCommandParameter(SequenceCommand c, int argumentNum, Random _rand, List<SequenceCommand> events) {
            switch (SequenceCommand.CommandParameters[c.CommandType]) {
                case SequenceCommandParameter.Bool:
                    return ((bool)c.Parameter ? 1 : 0);
                case SequenceCommandParameter.None:
                    return 0;
                case SequenceCommandParameter.NoteParam:
                    switch (argumentNum) {
                        case 0:
                            return (int)(c.Parameter as NoteParameter).Note;
                        case 1:
                            return (c.Parameter as NoteParameter).Velocity;
                        case 2:
                            return (int)(c.Parameter as NoteParameter).Length;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case SequenceCommandParameter.OpenTrack:
                    switch (argumentNum) {
                        case 0:
                            return (c.Parameter as OpenTrackParameter).TrackNumber;
                        case 1:
                            return (int)(c.Parameter as OpenTrackParameter).Index(events);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case SequenceCommandParameter.Random:
                    int argsNumR = NumArguments(c);
                    if (argsNumR == argumentNum + 1) {
                        return _rand.Next((c.Parameter as RandomParameter).Min, (c.Parameter as RandomParameter).Max);
                    } else {
                        return GetCommandParameter((c.Parameter as RandomParameter).Command, argumentNum, _rand, events);
                    }
                case SequenceCommandParameter.S16:
                    return (short)c.Parameter;
                case SequenceCommandParameter.Time:
                    int argsNumT = NumArguments(c);
                    if (argsNumT == argumentNum + 1) {
                        return (c.Parameter as TimeParameter).Value;
                    } else {
                        return GetCommandParameter((c.Parameter as TimeParameter).Command, argumentNum, _rand, events);
                    }
                case SequenceCommandParameter.TimeRandom:
                    int argsNumTR = NumArguments(c);
                    if (argsNumTR == argumentNum + 1) {
                        return _rand.Next((c.Parameter as RandomParameter).Min, (c.Parameter as RandomParameter).Max);
                    } else {
                        return GetCommandParameter((c.Parameter as RandomParameter).Command, argumentNum, _rand, events);
                    }
                case SequenceCommandParameter.TimeVariable:
                    int argsNumTV = NumArguments(c);
                    if (argsNumTV == argumentNum + 1) {
                        return (c.Parameter as VariableParameter).Variable;
                    } else {
                        return GetCommandParameter((c.Parameter as VariableParameter).Command, argumentNum, _rand, events);
                    }
                case SequenceCommandParameter.U16:
                    return (ushort)c.Parameter;
                case SequenceCommandParameter.U24:
                    return (c.Parameter as UInt24Parameter).Index(events);
                case SequenceCommandParameter.U8:
                    return (byte)c.Parameter;
                case SequenceCommandParameter.S8:
                    return (sbyte)c.Parameter;
                case SequenceCommandParameter.U8S16:
                    switch (argumentNum) {
                        case 0:
                            return (c.Parameter as U8S16Parameter).U8;
                        case 1:
                            return (c.Parameter as U8S16Parameter).S16;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case SequenceCommandParameter.Variable:
                    int argsNumV = NumArguments(c);
                    if (argsNumV == argumentNum + 1) {
                        return (c.Parameter as VariableParameter).Variable;
                    } else {
                        return GetCommandParameter((c.Parameter as VariableParameter).Command, argumentNum, _rand, events);
                    }
                case SequenceCommandParameter.VariableLength:
                    return (int)((uint)c.Parameter);
                case SequenceCommandParameter.If:
                    return GetCommandParameter((c.Parameter as SequenceCommand), argumentNum, _rand, events);
            }
            return 0;
        }
        public static int NumArguments(SequenceCommand c) {
            switch (SequenceCommand.CommandParameters[c.CommandType]) {
                case SequenceCommandParameter.Bool:
                    return 1;
                case SequenceCommandParameter.None:
                    return 0;
                case SequenceCommandParameter.NoteParam:
                    return 3;
                case SequenceCommandParameter.OpenTrack:
                    return 2;
                case SequenceCommandParameter.Random:
                    return NumArguments((c.Parameter as RandomParameter).Command);
                case SequenceCommandParameter.S16:
                    return 1;
                case SequenceCommandParameter.Time:
                    return NumArguments((c.Parameter as TimeParameter).Command) + 1;
                case SequenceCommandParameter.TimeRandom:
                    return NumArguments((c.Parameter as RandomParameter).Command) + 1;
                case SequenceCommandParameter.TimeVariable:
                    return NumArguments((c.Parameter as VariableParameter).Command) + 1;
                case SequenceCommandParameter.U16:
                    return 1;
                case SequenceCommandParameter.U24:
                    return 1;
                case SequenceCommandParameter.U8:
                    return 1;
                case SequenceCommandParameter.S8:
                    return 1;
                case SequenceCommandParameter.U8S16:
                    return 2;
                case SequenceCommandParameter.Variable:
                    return NumArguments((c.Parameter as VariableParameter).Command);
                case SequenceCommandParameter.VariableLength:
                    return 1;
                case SequenceCommandParameter.If:
                    return NumArguments(c.Parameter as SequenceCommand);
            }
            return 0;
        }
        private void ExecuteNext(int i) {
            ExecuteCommand(Events[_tracks[i].CurEvent], i);
        }
        private void ExecuteCommand(SequenceCommand c, int trackIndex) {
            Track track = _tracks[trackIndex];
            bool increment = true;
            int numArgs = NumArguments(c);
            int[] args = new int[numArgs];
            for (int i = 0; i < numArgs; i++) {
                args[i] = GetCommandParameter(c, i, _rand, Events);
            }
            if (c.CommandType == SequenceCommands.Variable || c.CommandType == SequenceCommands.TimeVariable) {
                args[args.Length - 1] = GetVar(args[args.Length - 1], trackIndex);
            }
            SequenceCommands trueCommandType = GetTrueCommandType(c);
            if (c.CommandType == SequenceCommands.If && !track.VariableFlag) {
                goto skip_processing;
            }
            switch (trueCommandType) {
                case SequenceCommands.Note: {
                    int duration = args[2];
                    int k = (int)args[0] + track.Transpose;
                    if (k < 0) {
                        k = 0;
                    } else if (k > 0x7F) {
                        k = 0x7F;
                    }
                    byte key = (byte)k;
                    PlayNote(track, key, (byte)args[1], duration);
                    track.PortamentoKey = key;
                    if (track.Mono) {
                        track.Rest = duration;
                        if (duration == 0) {
                            track.WaitingForNoteToFinishBeforeContinuingXD = true;
                        }
                    }
                    break;
                }
                case SequenceCommands.Wait:
                    track.Rest = args[0];
                    break;
                case SequenceCommands.ProgramChange:
                    track.Voice = args[0];
                    break;
                case SequenceCommands.OpenTrack:
                    if (trackIndex == 0) {
                        Track newTrack = _tracks[args[0]];
                        if (newTrack.Allocated && !newTrack.Enabled) {
                            newTrack.Enabled = true;
                            newTrack.CurEvent = args[1];
                        }
                    }
                    break;
                case SequenceCommands.Jump:
                    track.CurEvent = args[0];
                    increment = false;
                    break;
                case SequenceCommands.Call:
                    if (track.CallStackDepth < 3) {
                        track.CallStack[track.CallStackDepth] = track.CurEvent + 1;
                        track.CallStackDepth++;
                        track.CurEvent = args[0];
                        increment = false;
                    }
                    break;
                case SequenceCommands.Random:
                case SequenceCommands.Variable:
                case SequenceCommands.If:
                case SequenceCommands.Time:
                case SequenceCommands.TimeRandom:
                case SequenceCommands.TimeVariable:
                    throw new Exception("Gota messed up."); 
                case SequenceCommands.EnvHold:
                    track.Hold = (byte)args[0];
                    break;
                case SequenceCommands.BankSelect:
                    track.BankNum = args[0];
                    break;
                case SequenceCommands.Pan:
                    track.Panpot = (sbyte)(args[0] - 0x40);
                    break;
                case SequenceCommands.Volume:
                    track.Volume = (byte)args[0];
                    break;
                case SequenceCommands.MainVolume:
                    Volume = (byte)args[0];
                    break;
                case SequenceCommands.Transpose:
                    track.Transpose = (sbyte)args[0];
                    break;
                case SequenceCommands.PitchBend:
                    track.PitchBend = (sbyte)args[0];
                    break;
                case SequenceCommands.BendRange:
                    track.PitchBendRange = (byte)args[0];
                    break;
                case SequenceCommands.Prio:
                    track.Priority = (byte)args[0];
                    break;
                case SequenceCommands.NoteWait:
                    track.Mono = args[0] > 0;
                    break;
                case SequenceCommands.Tie:
                    track.Tie = args[0] > 0;
                    track.StopAllChannels();
                    break;
                case SequenceCommands.Porta: {
                    int k = args[0] + track.Transpose;
                    if (k < 0) {
                        k = 0;
                    } else if (k > 0x7F) {
                        k = 0x7F;
                    }
                    track.PortamentoKey = (byte)k;
                    track.Portamento = true;
                    break;
                }
                case SequenceCommands.ModDepth:
                    track.LFODepth = (byte)args[0];
                    break;
                case SequenceCommands.ModSpeed:
                    track.LFOSpeed = (byte)args[0];
                    break;
                case SequenceCommands.ModType:
                    track.LFOType = (LFOType)args[0];
                    break;
                case SequenceCommands.ModRange:
                    track.LFORange = (byte)args[0];
                    break;
                case SequenceCommands.PortaSw:
                    track.Portamento = args[0] > 0;
                    break;
                case SequenceCommands.PortaTime:
                    track.PortamentoTime = (byte)args[0];
                    break;
                case SequenceCommands.Attack:
                    track.Attack = (byte)args[0];
                    break;
                case SequenceCommands.Decay:
                    track.Decay = (byte)args[0];
                    break;
                case SequenceCommands.Sustain:
                    track.Sustain = (byte)args[0];
                    break;
                case SequenceCommands.Release:
                    track.Release = (byte)args[0];
                    break;
                case SequenceCommands.LoopStart:
                    if (track.CallStackDepth < 3) {
                        track.CallStack[track.CallStackDepth] = track.CurEvent;
                        track.CallStackLoops[track.CallStackDepth] = (byte)args[0];
                        track.CallStackDepth++;
                    }
                    break;
                case SequenceCommands.Volume2:
                    track.Expression = (byte)args[0];
                    break;
                case SequenceCommands.PrintVar:
                    Console.WriteLine("Variable " + args[0] + " = " + GetVar(args[0], trackIndex));
                    break;
                case SequenceCommands.ModDelay:
                    track.LFODelay = (ushort)args[0];
                    break;
                case SequenceCommands.Tempo:
                    _tempo = (ushort)args[0];
                    break;
                case SequenceCommands.SweepPitch:
                    track.SweepPitch = (short)args[0];
                    break;
                case SequenceCommands.LoopEnd:
                    if (track.CallStackDepth != 0) {
                        byte count = track.CallStackLoops[track.CallStackDepth - 1];
                        if (count != 0) {
                            count--;
                            if (count == 0) {
                                track.CallStackDepth--;
                                break;
                            }
                        }
                        track.CallStackLoops[track.CallStackDepth - 1] = count;
                        track.CurEvent = track.CallStack[track.CallStackDepth - 1];
                        increment = false;
                    }
                    break;
                case SequenceCommands.Return:
                    if (track.CallStackDepth != 0) {
                        track.CallStackDepth--;
                        track.CurEvent = track.CallStack[track.CallStackDepth];
                        increment = false;
                    }
                    break;
                case SequenceCommands.AllocateTrack:
                    if (track.Index == 0) {
                        for (int i = 0; i < 0x10; i++) {
                            if ((args[0] & (1 << i)) != 0) {
                                _tracks[i].Allocated = true;
                            }
                        }
                    }
                    break;
                case SequenceCommands.Fin:
                    track.Stopped = true;
                    increment = false;
                    break;
                case SequenceCommands.SetVar:
                    SetVar(args[0], trackIndex, (short)args[1]);
                    break;
                case SequenceCommands.AddVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) + args[1]));
                    break;
                case SequenceCommands.SubVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) - args[1]));
                    break;
                case SequenceCommands.MulVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) * args[1]));
                    break;
                case SequenceCommands.DivVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) / args[1]));
                    break;
                case SequenceCommands.ShiftVar:
                    SetVar(args[0], trackIndex, args[1] < 0 ? (short)(GetVar(args[0], trackIndex) >> -args[1]) : (short)(GetVar(args[0], trackIndex) << args[1]));
                    break;
                case SequenceCommands.RandVar: {
                    bool negate = false;
                    if (args[1] < 0) {
                        negate = true;
                        args[1] = (short)-args[1];
                    }
                    short val = (short)_rand.Next(args[1] + 1);
                    if (negate) {
                        val = (short)-val;
                    }
                    SetVar(args[0], trackIndex, val);
                    break;
                }
                case SequenceCommands.AndVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) & args[1]));
                    break;
                case SequenceCommands.OrVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) | (short)args[1]));
                    break;
                case SequenceCommands.XorVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) ^ args[1]));
                    break;
                case SequenceCommands.NotVar:
                    SetVar(args[0], trackIndex, (short)((~(GetVar(args[0], trackIndex) & args[1])) | (GetVar(args[0], trackIndex) & (~args[0]))));
                    break;
                case SequenceCommands.ModVar:
                    SetVar(args[0], trackIndex, (short)(GetVar(args[0], trackIndex) % args[1]));
                    break;
                case SequenceCommands.CmpEq:
                    track.VariableFlag = GetVar(args[0], trackIndex) == args[1];
                    break;
                case SequenceCommands.CmpGe:
                    track.VariableFlag = GetVar(args[0], trackIndex) >= args[1];
                    break;
                case SequenceCommands.CmpGt:
                    track.VariableFlag = GetVar(args[0], trackIndex) > args[1];
                    break;
                case SequenceCommands.CmpLe:
                    track.VariableFlag = GetVar(args[0], trackIndex) <= args[1];
                    break;
                case SequenceCommands.CmpLt:
                    track.VariableFlag = GetVar(args[0], trackIndex) < args[1];
                    break;
                case SequenceCommands.CmpNe:
                    track.VariableFlag = GetVar(args[0], trackIndex) != args[1];
                    break;
                case SequenceCommands.UserCall:
                    break;
                case SequenceCommands.Timebase:
                    _time.Stop();
                    Timebase = args[0];
                    _time.Start();
                    break;
                case SequenceCommands.Monophonic:
                case SequenceCommands.VelocityRange:
                case SequenceCommands.BiquadType:
                case SequenceCommands.BiquadValue:
                case SequenceCommands.ModPhase:
                case SequenceCommands.ModCurve:
                case SequenceCommands.FrontBypass:
                case SequenceCommands.SurroundPan:
                case SequenceCommands.LpfCutoff:
                case SequenceCommands.FxSendA:
                case SequenceCommands.FxSendB:
                case SequenceCommands.MainSend:
                case SequenceCommands.InitPan:
                case SequenceCommands.Mute:
                case SequenceCommands.FxSendC:
                case SequenceCommands.Damper:
                case SequenceCommands.ModPeriod:
                case SequenceCommands.EnvReset:
                case SequenceCommands.Mod2Curve:
                case SequenceCommands.Mod2Phase:
                case SequenceCommands.Mod2Depth:
                case SequenceCommands.Mod2Speed:
                case SequenceCommands.Mod2Type:
                case SequenceCommands.Mod2Range:
                case SequenceCommands.Mod2Delay:
                case SequenceCommands.Mod2Period:
                case SequenceCommands.Mod3Curve:
                case SequenceCommands.Mod3Phase:
                case SequenceCommands.Mod3Depth:
                case SequenceCommands.Mod3Speed:
                case SequenceCommands.Mod3Type:
                case SequenceCommands.Mod3Range:
                case SequenceCommands.Mod3Delay:
                case SequenceCommands.Mod3Period:
                case SequenceCommands.Mod4Curve:
                case SequenceCommands.Mod4Phase:
                case SequenceCommands.Mod4Depth:
                case SequenceCommands.Mod4Speed:
                case SequenceCommands.Mod4Type:
                case SequenceCommands.Mod4Range:
                case SequenceCommands.Mod4Delay:
                case SequenceCommands.Mod4Period:
                    Console.WriteLine("Command not implemented!");
                    break;
            }
            skip_processing:
            if (increment) {
                track.CurEvent++;
            }
        }
        public void Play() {
            if (State == PlayerState.Playing || State == PlayerState.Paused || State == PlayerState.Stopped) {
                Stop();
                InitEmulation();
                _tracks[0].CurEvent = currEventOverride;
                State = PlayerState.Playing;
                CreateThread();
            }
        }
        public void Pause() {
            if (State == PlayerState.Playing) {
                State = PlayerState.Paused;
                WaitThread();
            } else if (State == PlayerState.Paused || State == PlayerState.Stopped) {
                State = PlayerState.Playing;
                CreateThread();
            }
        }
        public void Stop() {
            if (State == PlayerState.Playing || State == PlayerState.Paused) {
                State = PlayerState.Stopped;
                WaitThread();
            }
        }
        public void Record(string fileName) {
            _mixer.CreateWaveWriter(fileName);
            InitEmulation();
            _tracks[0].CurEvent = currEventOverride;
            State = PlayerState.Recording;
            CreateThread();
            WaitThread();
            _mixer.CloseWaveWriter();
        }
        public void Dispose() {
            if (State == PlayerState.Playing || State == PlayerState.Paused || State == PlayerState.Stopped) {
                State = PlayerState.ShutDown;
                WaitThread();
            }
        }
        void SetTicks() {
            long[] totalTicks = new long[0x10];
            ReadTrackTicks(0, 0, currEventOverride, totalTicks);
            MaxTicks = totalTicks.Max();
            _longestTrack = totalTicks.ToList().IndexOf(MaxTicks);
        }
        void ReadTrackTicks(int trackNum, long baseTicks, int currEvent, long[] totalTicks) {
            bool noteWait = true;
            int[] callStack = new int[3];
            int callStackDepth = 0;
            List<int> readCommands = new List<int>();
            while (currEvent < Events.Count) {
                var c = Events[currEvent];
                if (c.Ticks[trackNum] == 0) {
                    c.Ticks[trackNum] = baseTicks;
                }
                int numArgs = NumArguments(c);
                int[] args = new int[numArgs];
                for (int i = 0; i < numArgs; i++) {
                    args[i] = GetCommandParameter(c, i, _rand, Events);
                }
                if (c.CommandType == SequenceCommands.Variable || c.CommandType == SequenceCommands.TimeVariable) {
                    args[args.Length - 1] = GetVar(args[args.Length - 1], trackNum);
                }
                SequenceCommands trueCommandType = GetTrueCommandType(c);
                switch (trueCommandType) {
                    case SequenceCommands.OpenTrack:
                        ReadTrackTicks(args[0], baseTicks, args[1], totalTicks);
                        break;
                    case SequenceCommands.NoteWait:
                        noteWait = args[0] > 0;
                        break;
                    case SequenceCommands.Note:
                        if (noteWait) {
                            baseTicks += args[2];
                        }
                        break;
                    case SequenceCommands.Wait:
                        baseTicks += args[0];
                        break;
                    case SequenceCommands.Call:
                        if (callStackDepth < 3) {
                            callStack[callStackDepth] = currEvent + 1;
                            callStackDepth++;
                            readCommands.Add(currEvent);
                            currEvent = args[0];
                            continue;
                        }
                        break;
                    case SequenceCommands.Jump:
                        if (!readCommands.Contains(args[0])) {
                            currEvent = args[0];
                            readCommands.Add(currEvent);
                            continue;
                        }
                        break;
                    case SequenceCommands.Return:
                        if (callStackDepth != 0) {
                            callStackDepth--;
                            readCommands.Add(currEvent);
                            currEvent = callStack[callStackDepth];
                            continue;
                        }
                        break;
                    case SequenceCommands.Fin:
                        totalTicks[trackNum] = baseTicks;
                        return;
                }
                readCommands.Add(currEvent);
                currEvent++;
            }
        }
        public static SequenceCommands GetTrueCommandType(SequenceCommand s) {
            switch (s.CommandType) { 
                case SequenceCommands.Random:
                case SequenceCommands.TimeRandom:
                    return GetTrueCommandType((s.Parameter as RandomParameter).Command);
                case SequenceCommands.Variable:
                case SequenceCommands.TimeVariable:
                    return GetTrueCommandType((s.Parameter as VariableParameter).Command);
                case SequenceCommands.If:
                    return GetTrueCommandType(s.Parameter as SequenceCommand);
                case SequenceCommands.Time:
                    return GetTrueCommandType((s.Parameter as TimeParameter).Command);
            }
            return s.CommandType;
        }
        public void SetCurrentPosition(long ticks) {
            if (State == PlayerState.Playing || State == PlayerState.Paused || State == PlayerState.Stopped) {
                if (State == PlayerState.Playing) {
                    Pause();
                }
                InitEmulation();
                while (true) {
                    if (ElapsedTicks == ticks) {
                        goto finish;
                    } else {
                        while (_tempoStack >= 240) {
                            _tempoStack -= 240;
                            for (int i = 0; i < 0x10; i++) {
                                Track track = _tracks[i];
                                if (track.Enabled && !track.Stopped) {
                                    track.Tick();
                                    while (track.Rest == 0 && !track.WaitingForNoteToFinishBeforeContinuingXD && !track.Stopped) {
                                        ExecuteNext(i);
                                    }
                                }
                            }
                            ElapsedTicks++;
                            if (ElapsedTicks == ticks) {
                                goto finish;
                            }
                        }
                        _tempoStack += _tempo;
                        _mixer.ChannelTick();
                        _mixer.EmulateProcess();
                    }
                }
            finish:
                for (int i = 0; i < 0x10; i++) {
                    _tracks[i].StopAllChannels();
                }
                Pause();
            }
        }
        public long GetCurrentPosition() => ElapsedTicks;
    }
    public enum PlayerState : byte {
        Stopped = 0,
        Playing,
        Paused,
        Recording,
        ShutDown
    }
    public delegate void SongEndedEvent();
}
