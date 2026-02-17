using GotaSoundIO.Sound.Playback;
using NAudio.Wave;
using System;

namespace GotaSequenceLib.Playback
{
    public class Mixer : IDisposable
    {
        private readonly float _samplesReciprocal;
        private readonly int _samplesPerBuffer;
        private bool _isFading;
        private long _fadeMicroFramesLeft;
        private float _fadePos;
        private float _fadeStepPerMicroframe;
        public Channel[] Channels;
        public bool[] Mutes = new bool[16];
        public float Volume = 1f;
        private readonly BufferedWaveProvider _buffer;
        private IWavePlayer _out;

        public Mixer()
        {
            const int sampleRate = 65456;
            _samplesPerBuffer = 341;
            _samplesReciprocal = 1f / _samplesPerBuffer;
            Channels = new Channel[0x10];
            for (byte i = 0; i < 0x10; i++)
            {
                Channels[i] = new Channel(i);
            }
            _buffer = new BufferedWaveProvider(new WaveFormat(sampleRate, 16, 2))
            {
                DiscardOnBufferOverflow = true,
                BufferLength = _samplesPerBuffer * 64,
            };
            Init(_buffer);
        }

        protected void Init(IWaveProvider waveProvider)
        {
            try
            {
                _out = new WasapiOut();
            }
            catch
            {
                _out = new NullWavePlayer();
            }
            _out.Init(waveProvider);
            _out.Play();
        }

        public void Dispose()
        {
            _out.Stop();
            _out.Dispose();
            CloseWaveWriter();
        }

        public Channel AllocateChannel(InstrumentType type, Track track)
        {
            ushort allowedChannels;
            switch (type)
            {
                case InstrumentType.PCM:
                    allowedChannels = 0b1111111111111111;
                    break;
                case InstrumentType.PSG:
                    allowedChannels = 0b0011111100000000;
                    break;
                case InstrumentType.Noise:
                    allowedChannels = 0b1100000000000000;
                    break;
                default:
                    return null;
            }
            static int GetScore(Channel c)
            {
                return c.Owner == null ? -2
                    : c.State == EnvelopeState.Release ? -1
                    : c.Owner.Priority;
            }
            Channel nChan = null;
            for (int i = 0; i < 0x10; i++)
            {
                if ((allowedChannels & (1 << i)) != 0)
                {
                    Channel c = Channels[i];
                    if (nChan != null)
                    {
                        int nScore = GetScore(nChan);
                        int cScore = GetScore(c);
                        if (cScore <= nScore && (cScore < nScore || c.Volume <= nChan.Volume))
                        {
                            nChan = c;
                        }
                    }
                    else
                    {
                        nChan = c;
                    }
                }
            }
            return nChan != null && track.Priority >= GetScore(nChan) ? nChan : null;
        }

        public void ChannelTick()
        {
            for (int i = 0; i < 0x10; i++)
            {
                Channel chan = Channels[i];
                if (chan.Owner != null)
                {
                    chan.StepEnvelope();
                    if (
                        chan.NoteDuration == 0
                        && !chan.Owner.WaitingForNoteToFinishBeforeContinuingXD
                    )
                    {
                        chan.State = EnvelopeState.Release;
                    }
                    int vol =
                        Utils.SustainTable[chan.NoteVelocity]
                        + chan.Velocity
                        + chan.Owner.GetVolume();
                    int pitch =
                        ((chan.Key - chan.BaseKey) << 6) + chan.SweepMain() + chan.Owner.GetPitch();
                    if (chan.State == EnvelopeState.Release && vol <= -92544)
                    {
                        chan.Stop();
                    }
                    else
                    {
                        chan.Volume = Utils.GetChannelVolume(vol);
                        chan.Timer = Utils.GetChannelTimer(chan.BaseTimer, pitch);
                        int p = chan.StartingPan + chan.Owner.GetPan();
                        if (p < -0x40)
                        {
                            p = -0x40;
                        }
                        else if (p > 0x3F)
                        {
                            p = 0x3F;
                        }
                        chan.Pan = (sbyte)p;
                    }
                }
            }
        }

        public void BeginFadeIn()
        {
            _fadePos = 0f;
            _fadeMicroFramesLeft = (long)(10000 / 1000.0 * 192);
            _fadeStepPerMicroframe = 1f / _fadeMicroFramesLeft;
            _isFading = true;
        }

        public void BeginFadeOut()
        {
            _fadePos = 1f;
            _fadeMicroFramesLeft = (long)(10000 / 1000.0 * 192);
            _fadeStepPerMicroframe = -1f / _fadeMicroFramesLeft;
            _isFading = true;
        }

        public bool IsFading()
        {
            return _isFading;
        }

        public bool IsFadeDone()
        {
            return _isFading && _fadeMicroFramesLeft == 0;
        }

        public void ResetFade()
        {
            _isFading = false;
            _fadeMicroFramesLeft = 0;
        }

        private WaveFileWriter _waveWriter;

        public void CreateWaveWriter(string fileName)
        {
            _waveWriter = new WaveFileWriter(fileName, _buffer.WaveFormat);
        }

        public void CloseWaveWriter()
        {
            _waveWriter?.Dispose();
        }

        public void EmulateProcess()
        {
            for (int i = 0; i < _samplesPerBuffer; i++)
            {
                for (int j = 0; j < 0x10; j++)
                {
                    Channel chan = Channels[j];
                    if (chan.Owner != null)
                    {
                        chan.EmulateProcess();
                    }
                }
            }
        }

        public void Process(bool output, bool recording)
        {
            float masterStep;
            float masterLevel;
            if (_isFading && _fadeMicroFramesLeft == 0)
            {
                masterStep = 0;
                masterLevel = 0;
            }
            else
            {
                float fromMaster = Volume;
                float toMaster = Volume;
                if (_fadeMicroFramesLeft > 0)
                {
                    const float scale = 10f / 6f;
                    fromMaster *= (_fadePos < 0f) ? 0f : (float)Math.Pow(_fadePos, scale);
                    _fadePos += _fadeStepPerMicroframe;
                    toMaster *= (_fadePos < 0f) ? 0f : (float)Math.Pow(_fadePos, scale);
                    _fadeMicroFramesLeft--;
                }
                masterStep = (toMaster - fromMaster) * _samplesReciprocal;
                masterLevel = fromMaster;
            }
            byte[] b = new byte[4];
            for (int i = 0; i < _samplesPerBuffer; i++)
            {
                int left = 0,
                    right = 0;
                for (int j = 0; j < 0x10; j++)
                {
                    Channel chan = Channels[j];
                    if (chan.Owner != null)
                    {
                        bool muted = Mutes[chan.Owner.Index];
                        chan.Process(out short channelLeft, out short channelRight);
                        if (!muted)
                        {
                            left += channelLeft;
                            right += channelRight;
                        }
                    }
                }
                float f = left * masterLevel;
                if (f < short.MinValue)
                {
                    f = short.MinValue;
                }
                else if (f > short.MaxValue)
                {
                    f = short.MaxValue;
                }
                left = (int)f;
                b[0] = (byte)left;
                b[1] = (byte)(left >> 8);
                f = right * masterLevel;
                if (f < short.MinValue)
                {
                    f = short.MinValue;
                }
                else if (f > short.MaxValue)
                {
                    f = short.MaxValue;
                }
                right = (int)f;
                b[2] = (byte)right;
                b[3] = (byte)(right >> 8);
                masterLevel += masterStep;
                if (output)
                {
                    _buffer.AddSamples(b, 0, 4);
                }
                if (recording)
                {
                    _waveWriter.Write(b, 0, 4);
                }
            }
        }
    }
}
