using Silk.NET.OpenAL;
using System;

namespace GotaSoundIO.Sound.Playback
{
    /// <summary>
    /// The one OpenAL device and context for the whole process.
    ///
    /// This has to be shared. OpenAL's current context is process-global, not per-object, so a
    /// second output that opened its own context would silently steal playback from the first:
    /// opening any window with sound in it would stop whatever was already playing, the two would
    /// bleed into each other, and closing one would set the current context to null and leave the
    /// app permanently silent. Every output instead shares this context and owns only its own
    /// source and buffers, which is what lets several editors play at once.
    ///
    /// The device stays open for the life of the process; there is nothing to gain from closing
    /// and reopening it, and doing so is exactly what caused the silence.
    /// </summary>
    internal static unsafe class OpenAlDevice
    {
        private static readonly object InitGate = new();
        private static Device* device;
        private static Context* context;
        private static bool initialized;
        private static bool unavailable;

        /// <summary>
        /// Serialises AL calls. Sources are independent, but they share one context, and the
        /// feeder threads of several editors would otherwise touch it concurrently.
        /// </summary>
        public static object Gate { get; } = new();

        public static AL Al { get; private set; }

        private static ALContext Alc { get; set; }

        /// <summary>
        /// Opens the device on first use. Throws if no device is available, which callers catch
        /// to fall back to <see cref="NullAudioOutput"/>.
        /// </summary>
        public static void Ensure()
        {
            lock (InitGate)
            {
                if (initialized)
                {
                    return;
                }
                if (unavailable)
                {
                    throw new InvalidOperationException("No OpenAL output device is available.");
                }

                AL al = AL.GetApi();
                ALContext alc = ALContext.GetApi();
                Device* newDevice = alc.OpenDevice("");
                if (newDevice is null)
                {
                    unavailable = true;
                    throw new InvalidOperationException("No OpenAL output device is available.");
                }
                Context* newContext = alc.CreateContext(newDevice, null);
                if (newContext is null)
                {
                    alc.CloseDevice(newDevice);
                    unavailable = true;
                    throw new InvalidOperationException("Could not create an OpenAL context.");
                }
                alc.MakeContextCurrent(newContext);

                Al = al;
                Alc = alc;
                device = newDevice;
                context = newContext;
                initialized = true;
            }
        }
    }
}
