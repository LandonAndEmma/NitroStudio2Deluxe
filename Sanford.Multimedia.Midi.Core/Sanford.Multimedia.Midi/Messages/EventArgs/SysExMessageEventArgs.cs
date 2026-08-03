namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// Class for exclusive system message events.
    /// </summary>
    public class SysExMessageEventArgs : MidiEventArgs
    {

        /// <summary>
        /// Main function for exclusive system message events.
        /// </summary>
        public SysExMessageEventArgs(SysExMessage message, int absoluteTicks = -1)
        {
            Message = message;
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
        /// Gets and returns the message.
        /// </summary>
        public SysExMessage Message { get; }
    }
}
