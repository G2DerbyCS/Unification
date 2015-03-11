using System;
using Unification.Models.Enums;

namespace Unification.Models
{
    /// <summary>
    /// An event args class for a loading type event.
    /// </summary>
    internal class LoadingCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// The LoadingState at which the load attempt ended.
        /// </summary>
        public LoadingState EndState
        {
            private set;
            get;
        }

        /// <summary>
        /// An exception, will be null if no exceptions were raised.
        /// </summary>
        public Exception Exception
        {
            private set;
            get;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="EndState">Level of loading completion.</param>
        /// <param name="Exception">Any exception raised whilst loading.</param>
        public LoadingCompletedEventArgs(LoadingState EndState, Exception Exception = null)
        {
            this.Exception = Exception;
            this.EndState  = EndState;
        }
    }
}
