using NAudio.Wave;
using System;

namespace GotaSoundIO.Sound.Playback
{
    /// <summary>
    /// Wraps a decoded stream and gives it the same looping the sequence player has.
    ///
    /// With the Loop box ticked the audio repeats from its loop point forever and never fades.
    /// Unticked it plays through, jumps back to the loop point once, then fades out over
    /// <see cref="FadeSeconds"/> and ends -- which is what Player does for a sequence when
    /// NumLoops is 0: it loops once and calls Mixer.BeginFadeOut.
    ///
    /// A file with no loop point of its own still loops; it just goes back to the start.
    /// </summary>
    public class LoopStream : WaveStream
    {
        /// <summary>Length of the fade after the final loop, matching the sequence mixer's.</summary>
        public const double FadeSeconds = 10;

        private readonly WaveStream sourceStream;
        private readonly StreamPlayer player;

        private bool hasLooped;
        private bool ended;
        private long fadeSamplesLeft = -1;
        private long fadeSamplesTotal;

        public LoopStream(
            StreamPlayer player,
            WaveStream sourceStream,
            uint loopStart,
            uint loopEnd
        )
        {
            this.player = player;
            this.sourceStream = sourceStream;
            LoopStart = loopStart;
            LoopEnd = loopEnd;
        }

        /// <summary>Sample to jump back to; 0 when the file carries no loop point.</summary>
        public uint LoopStart { get; set; }

        /// <summary>Sample to jump back from; the sample count when the file carries none.</summary>
        public uint LoopEnd { get; set; }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        private int BlockAlign => WaveFormat.Channels * (WaveFormat.BitsPerSample / 8);

        public uint CurrentSample
        {
            get => (uint)(sourceStream.Position / BlockAlign);
            set => sourceStream.Position = (long)value * BlockAlign;
        }

        public uint GetLengthInSamples => (uint)(sourceStream.Length / BlockAlign);

        /// <summary>True once the fade has run out and there is nothing left to play.</summary>
        public bool Ended => ended;

        /// <summary>True while the post-loop fade is running.</summary>
        public bool IsFading => fadeSamplesLeft >= 0;

        /// <summary>Starts over, as pressing Play again should.</summary>
        public void Restart()
        {
            hasLooped = false;
            ended = false;
            fadeSamplesLeft = -1;
            sourceStream.Position = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int total = 0;
            while (total < count && !ended)
            {
                // Never read past the loop end; whatever follows it is not part of the loop.
                long limit = Math.Min((long)LoopEnd * BlockAlign, sourceStream.Length);
                int room = (int)Math.Min(count - total, Math.Max(0, limit - sourceStream.Position));
                int read = room == 0 ? 0 : sourceStream.Read(buffer, offset + total, room);
                if (read <= 0)
                {
                    if (!Rewind())
                    {
                        break;
                    }
                    continue;
                }
                total += read;
            }
            ApplyFade(buffer, offset, total);
            return total;
        }

        /// <summary>Decides what happens at the loop end. False when playback is over.</summary>
        private bool Rewind()
        {
            if (player.Loop)
            {
                // Ticking the box mid-fade cancels it, so the sound comes back up to full.
                fadeSamplesLeft = -1;
                hasLooped = true;
                CurrentSample = LoopStart;
                return true;
            }
            if (hasLooped)
            {
                // Keep looping while the fade runs, exactly as the sequence player does: it goes
                // on repeating and only stops once Mixer.IsFadeDone. Without this a track
                // shorter than the fade would cut off abruptly at full volume.
                if (IsFading)
                {
                    CurrentSample = LoopStart;
                    return true;
                }
                ended = true;
                return false;
            }
            hasLooped = true;
            CurrentSample = LoopStart;
            fadeSamplesTotal = Math.Max(1, (long)(WaveFormat.SampleRate * FadeSeconds));
            fadeSamplesLeft = fadeSamplesTotal;
            return true;
        }

        /// <summary>Scales the frames just read by the falling fade envelope.</summary>
        private void ApplyFade(byte[] buffer, int offset, int bytes)
        {
            if (fadeSamplesLeft < 0 || bytes <= 0)
            {
                return;
            }
            int channels = WaveFormat.Channels;
            int frames = bytes / BlockAlign;
            for (int frame = 0; frame < frames; frame++)
            {
                double gain = Math.Max(0, (double)fadeSamplesLeft / fadeSamplesTotal);
                for (int channel = 0; channel < channels; channel++)
                {
                    int at = offset + (((frame * channels) + channel) * 2);
                    short sample = (short)(buffer[at] | (buffer[at + 1] << 8));
                    short faded = (short)(sample * gain);
                    buffer[at] = (byte)(faded & 0xFF);
                    buffer[at + 1] = (byte)((faded >> 8) & 0xFF);
                }
                if (fadeSamplesLeft > 0)
                {
                    fadeSamplesLeft--;
                }
            }
            if (fadeSamplesLeft == 0)
            {
                ended = true;
            }
        }
    }
}
