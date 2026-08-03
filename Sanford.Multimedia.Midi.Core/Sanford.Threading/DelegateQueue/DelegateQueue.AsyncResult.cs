using Sanford.Multimedia.Midi.Core.Sanford.Threading;
using System;

namespace Sanford.Threading
{
    public partial class DelegateQueue
    {
        private enum NotificationType
        {
            None,
            BeginInvokeCompleted,
            PostCompleted
        }

        /// <summary>
        /// Implements the IAsyncResult interface for the DelegateQueue class.
        /// </summary>
        private class DelegateQueueAsyncResult : AsyncResult
        {
            // The delegate to be invoked.

            // Args to be passed to the delegate.
            private readonly object[] args;

            // The object returned from the delegate.

            // Represents a possible exception thrown by invoking the method.

            public DelegateQueueAsyncResult(
                object owner,
                Delegate method,
                object[] args,
                bool synchronously,
                NotificationType notificationType)
                : base(owner, null, null)
            {
                Method = method;
                this.args = args;
                NotificationType = notificationType;
            }

            public DelegateQueueAsyncResult(
                object owner,
                AsyncCallback callback,
                object state,
                Delegate method,
                object[] args,
                bool synchronously,
                NotificationType notificationType)
                : base(owner, callback, state)
            {
                Method = method;
                this.args = args;
                NotificationType = notificationType;
            }

            public void Invoke()
            {
                try
                {
                    ReturnValue = Method.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
                finally
                {
                    Signal();
                }
            }

            public object[] GetArgs()
            {
                return args;
            }

            public object ReturnValue { get; private set; } = null;

            public Exception Error { get; set; } = null;

            public Delegate Method { get; }

            public NotificationType NotificationType { get; }
        }
    }
}
