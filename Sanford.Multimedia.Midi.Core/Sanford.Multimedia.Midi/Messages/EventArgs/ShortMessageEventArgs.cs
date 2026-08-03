namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// Raw short message as int or byte array, useful when working with VST.
    /// </summary>
    public class ShortMessageEventArgs : MidiEventArgs
    {

        /// <summary>
        /// A short message event that calculates the absolute ticks.
        /// </summary>
        public ShortMessageEventArgs(ShortMessage message, int absoluteTicks = -1)
        {
            Message = message;
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
		/// A short message event that uses a timestamp and calculates the absolute ticks.
		/// </summary>
        public ShortMessageEventArgs(int message, int timestamp = 0, int absoluteTicks = -1)
        {
            Message = new ShortMessage(message)
            {
                Timestamp = timestamp
            };
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
		/// A short message event that calculates the status byte, data 1 byte, data 2 byte, and absolute ticks.
		/// </summary>
        public ShortMessageEventArgs(byte status, byte data1, byte data2, int absoluteTicks = -1)
        {
            Message = new ShortMessage(status, data1, data2);
            AbsoluteTicks = absoluteTicks;
        }

        /// <summary>
		/// Gets and returns the message.
		/// </summary>
        public ShortMessage Message { get; }

        /// <summary>
		/// Returns the channel message event.
		/// </summary>
        public static ShortMessageEventArgs FromChannelMessage(ChannelMessageEventArgs arg)
        {
            return new ShortMessageEventArgs(arg.Message);
        }

        /// <summary>
		/// Returns the common system message event.
		/// </summary>
        public static ShortMessageEventArgs FromSysCommonMessage(SysCommonMessageEventArgs arg)
        {
            return new ShortMessageEventArgs(arg.Message);
        }

        /// <summary>
		/// Returns the realtime system message event.
		/// </summary>
        public static ShortMessageEventArgs FromSysRealtimeMessage(SysRealtimeMessageEventArgs arg)
        {
            return new ShortMessageEventArgs(arg.Message);
        }
    }
}
