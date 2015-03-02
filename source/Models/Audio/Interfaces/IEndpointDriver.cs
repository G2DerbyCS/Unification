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
        /// Retrieves the current available capacity of the audio framebuffer associated with the endpoint.
        /// </summary>
        int AvailableFramebufferCapacity { get; }

        /// <summary>
        /// Retrieves the number of bytes accepted as an audio frame by the endpoint.
        /// </summary>
        int BytesPerFrame { get; }

        /// <summary>
        /// Indicates currently selected Endpoint.
        /// </summary>
        Endpoint Endpoint { get; }

        /// <summary>
        /// Gets the output Endpoints available to this IEndpointDriver instance. 
        /// </summary>
        IEnumerable<Endpoint> GetAvailableEndpoints();

        /// <summary>
        /// Retrieves the maximum available capacity of the audio framebuffer associated with the endpoint.
        /// </summary>
        int MaxFramebufferCapacity { get; }

        /// <summary>
        /// Places byte array into device memory for rendering.
        /// </summary>
        /// <param name="AudioFramebuffer">Byte array containing audio data.</param>
        /// <param name="BytesToRead">Number of bytes written to AudioFramebuffer byte array.</param>
        /// <param name="FrameCount">Number of frames contained in AudioFramebuffer byte array.</param>
        bool RenderFramebuffer(byte[] AudioFramebuffer, int BytesToRead, int FrameCount);

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
