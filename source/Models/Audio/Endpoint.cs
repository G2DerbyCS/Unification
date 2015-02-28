using System;

namespace Unification.Models.Audio
{
    /// <summary>
    /// A class for enumerating endpoints (capture/output devices).
    /// </summary>
    internal class Endpoint
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Title">Enpoint friendly name.</param>
        /// <param name="Guid">Endpoint device guid.</param>
        Endpoint(string Title, Guid Guid)
        {
            this.Guid  = Guid;
            this.Title = Title;
        }

        /// <summary>
        /// Endpoint device guid.
        /// </summary>
        Guid Guid
        {
            private set;
            get;
        }

        /// <summary>
        /// Enpoint friendly name.
        /// </summary>
        String Title 
        {
            private set;
            get; 
        }
    }
}
