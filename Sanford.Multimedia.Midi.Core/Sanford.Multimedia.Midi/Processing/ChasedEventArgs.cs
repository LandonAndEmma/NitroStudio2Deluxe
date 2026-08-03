using System;
using System.Collections;

namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Processing
{
    /// <summary>
    /// A class for chased events.
    /// </summary>
    public class ChasedEventArgs : EventArgs
    {

        /// <summary>
        /// Main function for chased events.
        /// </summary>
        public ChasedEventArgs(ICollection messages)
        {
            Messages = messages;
        }

        /// <summary>
		/// Gets and returns messages.
		/// </summary>
        public ICollection Messages { get; }
    }
}
