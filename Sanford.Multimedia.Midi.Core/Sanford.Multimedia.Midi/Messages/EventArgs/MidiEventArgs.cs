using SystemEventArgs = System.EventArgs;

namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// Class for MIDI events.
    /// </summary>
    public class MidiEventArgs : SystemEventArgs
    {
        /// <summary>
        /// Gets and sets the ticks for the MIDI events.
        /// </summary>
        public int AbsoluteTicks { get; set; }
    }
}
