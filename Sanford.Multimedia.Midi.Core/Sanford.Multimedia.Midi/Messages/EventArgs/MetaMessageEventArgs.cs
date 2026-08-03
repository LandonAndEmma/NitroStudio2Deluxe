namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// Class for declaring metadata message events.
    /// </summary>
    public class MetaMessageEventArgs : MidiEventArgs
    {

        /// <summary>
        /// Main function for declaring metadata message events.
        /// </summary>
        public MetaMessageEventArgs(MetaMessage message, int absoluteTicks = -1)
        {
            Message = message;
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
        /// Gets and returns the message.
        /// </summary>
        public MetaMessage Message { get; }
    }
}
