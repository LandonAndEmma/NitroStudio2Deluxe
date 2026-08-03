#region License

/* Copyright (c) 2007 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

#endregion

#region Contact

/*
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */

#endregion

using System;
using System.Diagnostics;

namespace Sanford.Multimedia.Midi.Core.Sanford.Threading.DelegateScheduler
{
    /// <summary>
    /// Indicates the tasks to be compared.
    /// </summary>
    public class Task : IComparable
    {
        #region Task Members

        #region Fields

        // The number of times left to invoke the delegate associated with this Task.

        // The interval between delegate invocation.

        // The delegate to invoke.

        // The arguments to pass to the delegate when it is invoked.
        private readonly object[] args;

        // The time for the next timeout;

        // For locking.
        private readonly object lockObject = new();

        #endregion

        #region Construction

        internal Task(
            int count,
            int millisecondsTimeout,
            Delegate method,
            object[] args)
        {
            Count = count;
            MillisecondsTimeout = millisecondsTimeout;
            Method = method;
            this.args = args;

            ResetNextTimeout();
        }

        #endregion

        #region Methods

        internal void ResetNextTimeout()
        {
            NextTimeout = DateTime.Now.AddMilliseconds(MillisecondsTimeout);
        }

        internal object Invoke(DateTime signalTime)
        {
            Debug.Assert(Count is DelegateScheduler.Infinite or > 0);

            object returnValue = Method.DynamicInvoke(args);

            if (Count == DelegateScheduler.Infinite)
            {
                NextTimeout = NextTimeout.AddMilliseconds(MillisecondsTimeout);
            }
            else
            {
                Count--;

                if (Count > 0)
                {
                    NextTimeout = NextTimeout.AddMilliseconds(MillisecondsTimeout);
                }
            }

            return returnValue;
        }

        /// <summary>
		/// Initializes returns the arguments.
		/// </summary>
        public object[] GetArgs()
        {
            return args;
        }

        #endregion

        #region Properties

        /// <summary>
		/// Gets and returns the next timeout.
		/// </summary>
        public DateTime NextTimeout { get; private set; }

        /// <summary>
		/// Gets and returns the count.
		/// </summary>
        public int Count { get; private set; }

        /// <summary>
		/// Gets and returns the method.
		/// </summary>
        public Delegate Method { get; }

        /// <summary>
		/// Gets and returns the timeout in milliseconds.
		/// </summary>
        public int MillisecondsTimeout { get; }

        #endregion

        #endregion

        #region IComparable Members


        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer indicates whenever the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// Compares between the subtracted next timeout and the task.
        /// </returns>
        public int CompareTo(object obj)
        {
            return obj is not Task t ? throw new ArgumentException("obj is not the same type as this instance.") : -NextTimeout.CompareTo(t.NextTimeout);
        }

        #endregion
    }
}
