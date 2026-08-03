namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// This class declares invalid short message events.
    /// </summary>
    public class InvalidShortMessageEventArgs : MidiEventArgs
    {

        /// <summary>
        /// Main function for when the invalid short message event is declared.
        /// </summary>
        public InvalidShortMessageEventArgs(int message, int absoluteTicks = -1)
        {
            Message = message;
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
        /// Gets and returns the message.
        /// </summary>
        public int Message { get; }
    }
}
