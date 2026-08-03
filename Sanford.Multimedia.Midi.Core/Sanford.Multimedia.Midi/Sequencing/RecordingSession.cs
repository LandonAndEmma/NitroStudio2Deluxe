using Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Clocks;
using Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Messages;
using System.Collections.Generic;

namespace Sanford.Multimedia.Midi.Core.Sanford.Multimedia.Midi.Sequencing
{
    /// <summary>
    /// This class initializes the recording sessions.
    /// </summary>
    public class RecordingSession
    {
        private readonly IClock clock;

        private readonly List<TimestampedMessage> buffer = [];

        /// <summary>
        /// Main function for the recording sessions.
        /// </summary>
        public RecordingSession(IClock clock)
        {
            this.clock = clock;
        }

        /// <summary>
		/// Builds the tracks, sorts and compares between a buffer and a timestamp, then creates a timestamped message with the amount of ticks.
		/// </summary>
        public void Build()
        {
            Result = new Track();

            buffer.Sort(new TimestampComparer());

            foreach (TimestampedMessage tm in buffer)
            {
                Result.Insert(tm.ticks, tm.message);
            }
        }

        /// <summary>
		/// Removes all elements from the list.
		/// </summary>
        public void Clear()
        {
            buffer.Clear();
        }

        /// <summary>
		/// Gets and returns the track result for the recording session.
		/// </summary>
        public Track Result { get; private set; } = new Track();

        /// <summary>
		/// Records a channel message if the clock is running.
		/// </summary>
        public void Record(ChannelMessage message)
        {
            if (clock.IsRunning)
            {
                buffer.Add(new TimestampedMessage(clock.Ticks, message));
            }
        }

        /// <summary>
		/// Records an external system message if the clock is running.
		/// </summary>
        public void Record(SysExMessage message)
        {
            if (clock.IsRunning)
            {
                buffer.Add(new TimestampedMessage(clock.Ticks, message));
            }
        }

        private struct TimestampedMessage
        {
            public int ticks;

            public IMidiMessage message;

            public TimestampedMessage(int ticks, IMidiMessage message)
            {
                this.ticks = ticks;
                this.message = message;
            }
        }

        private class TimestampComparer : IComparer<TimestampedMessage>
        {
            #region IComparer<TimestampedMessage> Members

            public int Compare(TimestampedMessage x, TimestampedMessage y)
            {
                return x.ticks > y.ticks ? 1 : x.ticks < y.ticks ? -1 : 0;
            }

            #endregion
        }
    }
}
