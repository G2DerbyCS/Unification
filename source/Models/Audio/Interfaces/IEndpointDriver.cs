using NAudio.Wave;
using System;
using System.Collections.Generic;
using Unification.Models.Audio.Enums;

namespace Unification.Models.Audio.Interfaces
{
    /// <summary>
    /// Generic interface for all EnpointDrivers.
    /// </summary>
    internal interface IEndpointDriver : IDisposable
    {
        /// <summary>
        /// Indicates currently selected Endpoint.
        /// </summary>
        Endpoint Endpoint { get; }

        /// <summary>
        /// Gets the output Endpoints available to this IEndpointDriver instance. 
        /// </summary>
        IEnumerable<Endpoint> GetAvailableEndpoints();

        /// <summary>
        /// Fills Endpoint render buffer with data from the WaveProvider.
        /// </summary>
        /// <param name="WaveProvider">WaveProvider to be read.</param>
        /// <param name="FrameCount">Number of frames to read from WaveProvider.</param>
        void ReadWaveProvider(IWaveProvider WaveProvider, int FrameCount);

        /// <summary>
        /// Assings Endpoint for audio output.
        /// </summary>
        /// <param name="Endpoint">Audio ouput endpoint.</param>
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
