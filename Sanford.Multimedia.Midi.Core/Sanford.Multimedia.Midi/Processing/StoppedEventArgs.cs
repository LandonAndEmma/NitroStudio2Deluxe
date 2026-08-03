using System;
using System.Collections;

namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Processing
{
    /// <summary>
    /// A class for stopped events.
    /// </summary>
    public class StoppedEventArgs : EventArgs
    {

        /// <summary>
        /// Main function for stopped events.
        /// </summary>
        public StoppedEventArgs(ICollection messages)
        {
            Messages = messages;
        }

        /// <summary>
		/// Gets and returns messages.
		/// </summary>
        public ICollection Messages { get; }
    }
}
