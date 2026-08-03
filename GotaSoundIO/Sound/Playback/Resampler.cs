using System;

namespace GotaSoundIO.Sound.Playback
{
    /// <summary>
    /// Converts 16-bit PCM to the fixed output rate the audio device is opened at, and mono up to
    /// stereo, so every platform hands the sound card the same bytes.
    ///
    /// This matters because the DS mixer runs at 65456 Hz, which no sound card plays. Something has
    /// to resample, and letting the OS do it means Windows, PulseAudio and CoreAudio each produce
    /// audibly different results. Doing it here, with fixed coefficients, makes the output depend
    /// only on the input.
    ///
    /// The filter is a windowed-sinc polyphase FIR: a table of <see cref="Phases"/> sub-sample
    /// offsets, each holding <see cref="Taps"/> coefficients, built once per rate pair. When
    /// downsampling the sinc cutoff drops to the output Nyquist so nothing aliases back down —
    /// 65456 to 48000 is a 1.36x reduction, which cheaper interpolation turns into audible grit on
    /// PSG channels and bright samples.
    ///
    /// Nothing here touches file export: rendering to WAV writes the mixer's raw 65456 Hz frames
    /// straight out, and never reaches this class.
    /// </summary>
    public sealed class Resampler
    {
        /// <summary>Filter length. 32 taps gives a stopband deep enough for 16-bit material.</summary>
        private const int Taps = 32;

        private const int Half = Taps / 2;

        /// <summary>Sub-sample positions the fractional offset is quantised to.</summary>
        private const int Phases = 256;

        private readonly float[] coefficients; // Phases * Taps, phase-major
        private readonly int sourceChannels;

        /// <summary>Input frames, interleaved, including the left context the filter needs.</summary>
        private float[] buffer;
        private int bufferFrames;

        /// <summary>
        /// Output frames emitted so far. Read positions are derived from this with integer
        /// arithmetic rather than accumulated as a running double, because a running total drifts
        /// by a fraction of a sample depending on how the input happens to be chunked — and a
        /// live device delivers uneven chunks, which would make playback differ from a single
        /// pass over the same audio.
        /// </summary>
        private long outputIndex;

        /// <summary>Global input frame index sitting at buffer[0]; starts negative for priming.</summary>
        private long bufferStart;

        public Resampler(int sourceRate, int sourceChannels, int targetRate)
        {
            this.sourceChannels = sourceChannels;
            SourceRate = sourceRate;
            TargetRate = targetRate;

            // Cut at whichever Nyquist is lower: the output's when downsampling, the input's
            // otherwise (where no filtering beyond plain interpolation is needed).
            double cutoff = Math.Min(1.0, (double)targetRate / sourceRate);
            coefficients = BuildCoefficients(cutoff);

            buffer = new float[(Taps + 4096) * sourceChannels];
            Reset();
        }

        public int SourceRate { get; }

        public int TargetRate { get; }

        /// <summary>Output is always stereo, so mono sources are duplicated across both channels.</summary>
        public const int TargetChannels = 2;

        /// <summary>Clears the filter history. Call when seeking or restarting playback.</summary>
        public void Reset()
        {
            Array.Clear(buffer, 0, buffer.Length);
            // Prime with Half frames of silence so the first real sample has left context.
            bufferFrames = Half;
            bufferStart = -Half;
            outputIndex = 0;
        }

        /// <summary>
        /// Resamples one block. Returns interleaved stereo 16-bit frames at the target rate;
        /// the count varies slightly between calls as the fractional position advances.
        /// </summary>
        public short[] Process(ReadOnlySpan<short> input)
        {
            int inputFrames = input.Length / sourceChannels;
            Append(input, inputFrames);

            // Every output needs Half frames of right context, so stop before running off the end.
            long limit = bufferStart + bufferFrames - Half;
            int capacity = (int)Math.Max(
                0,
                (((limit - SourcePosition(outputIndex)) * TargetRate) / SourceRate) + 2
            );
            short[] output = new short[capacity * TargetChannels];

            int written = 0;
            while (SourcePosition(outputIndex) < limit)
            {
                long globalIndex = SourcePosition(outputIndex);
                int index = (int)(globalIndex - bufferStart);
                int phase = PhaseOf(outputIndex);
                outputIndex++;
                int coefficientBase = phase * Taps;

                float left = 0f;
                float right = 0f;
                for (int tap = 0; tap < Taps; tap++)
                {
                    float weight = coefficients[coefficientBase + tap];
                    int frame = index - Half + 1 + tap;
                    int sample = frame * sourceChannels;
                    left += buffer[sample] * weight;
                    right += buffer[sample + (sourceChannels > 1 ? 1 : 0)] * weight;
                }

                output[written++] = Clamp(left);
                output[written++] = Clamp(right);
            }

            Compact();
            return written == output.Length ? output : output[..written];
        }

        /// <summary>Appends new input frames, growing the working buffer if needed.</summary>
        private void Append(ReadOnlySpan<short> input, int inputFrames)
        {
            int needed = (bufferFrames + inputFrames) * sourceChannels;
            if (needed > buffer.Length)
            {
                Array.Resize(ref buffer, needed * 2);
            }
            int offset = bufferFrames * sourceChannels;
            for (int i = 0; i < input.Length; i++)
            {
                buffer[offset + i] = input[i];
            }
            bufferFrames += inputFrames;
        }

        /// <summary>Input frame index the given output frame reads from. Exact, never drifts.</summary>
        private long SourcePosition(long output) => output * SourceRate / TargetRate;

        /// <summary>Sub-sample offset of the given output frame, as a polyphase table index.</summary>
        private int PhaseOf(long output)
        {
            long remainder = output * SourceRate % TargetRate;
            return (int)(remainder * Phases / TargetRate);
        }

        /// <summary>Drops consumed frames, keeping the left context the next block needs.</summary>
        private void Compact()
        {
            int keepFrom = (int)(SourcePosition(outputIndex) - bufferStart) - Half;
            if (keepFrom <= 0)
            {
                return;
            }
            int remaining = bufferFrames - keepFrom;
            Array.Copy(
                buffer,
                keepFrom * sourceChannels,
                buffer,
                0,
                remaining * sourceChannels
            );
            bufferFrames = remaining;
            bufferStart += keepFrom;
        }

        private static short Clamp(float value)
        {
            // Round-half-to-even, which is what Math.Round does by default and is deterministic.
            double rounded = Math.Round(value);
            return rounded >= short.MaxValue ? short.MaxValue
                : rounded <= short.MinValue ? short.MinValue
                : (short)rounded;
        }

        /// <summary>
        /// Builds the polyphase table: for each sub-sample offset, a Blackman-windowed sinc,
        /// normalised so every phase has unity DC gain and the output level cannot drift.
        /// </summary>
        private static float[] BuildCoefficients(double cutoff)
        {
            float[] table = new float[Phases * Taps];
            for (int phase = 0; phase < Phases; phase++)
            {
                double fraction = (double)phase / Phases;
                double sum = 0;
                int start = phase * Taps;
                for (int tap = 0; tap < Taps; tap++)
                {
                    double x = tap - Half + 1 - fraction;
                    double value = cutoff * Sinc(cutoff * x) * Blackman(x / Half);
                    table[start + tap] = (float)value;
                    sum += value;
                }
                if (sum != 0)
                {
                    for (int tap = 0; tap < Taps; tap++)
                    {
                        table[start + tap] = (float)(table[start + tap] / sum);
                    }
                }
            }
            return table;
        }

        private static double Sinc(double x) =>
            Math.Abs(x) < 1e-12 ? 1.0 : Math.Sin(Math.PI * x) / (Math.PI * x);

        /// <summary>Blackman window over x in [-1, 1], zero outside.</summary>
        private static double Blackman(double x) =>
            Math.Abs(x) >= 1
                ? 0
                : 0.42 + (0.5 * Math.Cos(Math.PI * x)) + (0.08 * Math.Cos(2 * Math.PI * x));
    }
}
