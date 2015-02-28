using NAudio.Wave;
using System;
using System.Collections.Generic;
using Unification.Models.Audio.Enums;

namespace Unification.Models.Audio.Interfaces
{
    /// <summary>
    /// Generic interface for all EnpointDrivers.
    /// </summary>
    internal interface IEndpointDriver
    {
        /// <summary>
        /// Gets the output Endpoints available to this IEndpointDriver instance. 
        /// </summary>
        IEnumerable<Endpoint> AvailableEndpoints { get; }

        /// <summary>
        /// Parses buffer content and outputs content on active Endpoint.
        /// </summary>
        /// <param name="Buffer">Buffer containing audio data.</param>
        /// <returns></returns>
        bool ParseBuffer(byte[] InputBuffer);

        /// <summary>
        /// Indicates currently selected Endpoint.
        /// </summary>
        Endpoint Endpoint { get; }

        /// <summary>
        /// Assings Endpoint for audio output.
        /// </summary>
        /// <param name="Endpoint"></param>
        void SetEndpoint(Endpoint Endpoint);

        /// <summary>
        /// Indicates the availability of this IEndpointDriver instance.
        /// </summary>
        EndpointDriverState State { get; }

        /// <summary>
        /// Event to be raised when the State property changes.
        /// </summary>
        event EventHandler<EndpointDriverStateChangeEventArgs> StateChangedEvent;
    }
}
