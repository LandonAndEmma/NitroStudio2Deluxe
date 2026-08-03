using SystemEventArgs = System.EventArgs;

namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages.EventArgs
{
    /// <summary>
    /// Class for system realtime message events.
    /// </summary>
    public class SysRealtimeMessageEventArgs : SystemEventArgs
    {
        /// <summary>
        /// Requests the start for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs Start = new(SysRealtimeMessage.StartMessage);

        /// <summary>
        /// Requests to continue for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs Continue = new(SysRealtimeMessage.ContinueMessage);

        /// <summary>
        /// Requests to stop for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs Stop = new(SysRealtimeMessage.StopMessage);

        /// <summary>
        /// Requests the clock for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs Clock = new(SysRealtimeMessage.ClockMessage);

        /// <summary>
        /// Requests the ticks for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs Tick = new(SysRealtimeMessage.TickMessage);

        /// <summary>
        /// Requests the active sense for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs ActiveSense = new(SysRealtimeMessage.ActiveSenseMessage);

        /// <summary>
        /// Requests to restart for the system realtime message event.
        /// </summary>
        public static readonly SysRealtimeMessageEventArgs Reset = new(SysRealtimeMessage.ResetMessage);

        private SysRealtimeMessageEventArgs(SysRealtimeMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets and returns the message.
        /// </summary>
        public SysRealtimeMessage Message { get; }
    }
}
