namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// Class for system common message events.
    /// </summary>
    public class SysCommonMessageEventArgs : MidiEventArgs
    {

        /// <summary>
        /// Main function for system common message events.
        /// </summary>
        public SysCommonMessageEventArgs(SysCommonMessage message, int absoluteTicks = -1)
        {
            Message = message;
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
        /// Gets and returns the message.
        /// </summary>
        public SysCommonMessage Message { get; }
    }
}
