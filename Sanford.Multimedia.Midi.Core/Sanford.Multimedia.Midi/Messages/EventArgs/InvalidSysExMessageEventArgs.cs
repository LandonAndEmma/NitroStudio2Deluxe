using System.Collections;

namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// This class declares invalid exclusive system message events.
    /// </summary>
    public class InvalidSysExMessageEventArgs : MidiEventArgs
    {
        private readonly byte[] messageData;

        /// <summary>
        /// Main function for declared invalid exclusive system message events.
        /// </summary>
        public InvalidSysExMessageEventArgs(byte[] messageData, int absoluteTicks = -1)
        {
            this.messageData = messageData;
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
        /// Gets and returns the message data.
        /// </summary>
        public ICollection MessageData => messageData;
    }
}
