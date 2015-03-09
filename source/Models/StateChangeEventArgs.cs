using System;

namespace Unification.Models
{
    /// <summary>
    /// An EventArgs class for a change in object state.
    /// </summary>
    /// <typeparam name="T">Type specifier.</typeparam>
    public class StateChangeEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Current object state.
        /// </summary>
        public readonly T CurrentState;

        /// <summary>
        /// Previous object state.
        /// </summary>
        public readonly T PreviousState;

        /// <summary>
        /// An exception, will be null if no exceptions were raised.
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="PreviousState">Object's previous state.</param>
        /// <param name="CurrentState">Object's current state.</param>
        /// /// <param name="Exception">Raised exception.</param>
        public StateChangeEventArgs(T CurrentState, T PreviousState = default(T), Exception Exception = null)
        {
            this.CurrentState  = CurrentState;
            this.Exception     = Exception;
            this.PreviousState = PreviousState;
        }
    }
}
