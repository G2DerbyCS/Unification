using System;
using Unification.Models.Audio.Enums;

namespace Unification.Models.Audio
{
    /// <summary>
    /// IEndpointDriver StateChangedEvent event arguments.
    /// </summary>
    internal class EndpointDriverStateChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="State">IEndpointDriver state.</param>
        /// <param name="Exception">Raised exception.</param>
        public EndpointDriverStateChangeEventArgs(EndpointDriverState State, Exception Exception = null)
        {
            this.Exception = Exception;
            this.State     = State;
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
        /// Indicates IEndpointDriver instance state.
        /// </summary>
        public EndpointDriverState State 
        {
            private set;
            get; 
        }
    }
}
