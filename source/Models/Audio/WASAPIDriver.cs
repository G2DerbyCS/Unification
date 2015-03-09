using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unification.Models.Audio.Enums;
using Unification.Models.Audio.Interfaces;

namespace Unification.Models.Audio
{
    /// <summary>
    /// An IEndpoint driver implimentation based on NAudio's Wasapi WasapiOut class.
    /// </summary>
    internal class WASAPIDriver : IEndpointDriver
    {
        private AudioClient                   _AudioClient          = null;
        private readonly AudioClientShareMode _AudioClientShareMode = AudioClientShareMode.Shared;
        private readonly int                  _BufferDuration       = 10000000;
        private int                           _BytesPerFrame        = -1;
        private Endpoint                      _Endpoint             = null;
        private EndpointDriverState           _EndpointDriverState  = EndpointDriverState.Unavailable;
        private MMDevice                      _MMDevice             = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WASAPIDriver() : this(null)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Endpoint">Target audio output endpoint.</param>
        public WASAPIDriver(Endpoint Endpoint)
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                Exception NSE = new NotSupportedException("WASAPI supported only on Windows Vista and above");

                State = EndpointDriverState.Unsupported;

                if (StateChangedEvent != null)
                    StateChangedEvent(this, new StateChangeEventArgs<EndpointDriverState>(State, EndpointDriverState.Unavailable, NSE));

                throw NSE;
            }

            if (Endpoint == null)
            {
                AssignDefaultEndpoint();
            }
            else
            {
                this.Endpoint = Endpoint;
            }

            OpenAudioClient();
        }

        /// <summary>
        /// Utilizes a MMDeviceEnumerator to find and assign the current default system media output device.
        /// </summary>
        private void AssignDefaultEndpoint()
        {
            _MMDevice = (new MMDeviceEnumerator()).GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _Endpoint = new Endpoint(_MMDevice.FriendlyName, _MMDevice.GetGuid());
        }

        /// <summary>
        /// Utalizes a MMDeviceEnumerator to find and assign an MMDevice that matches the Enpoint GUID.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown if no MMDevice with a matching GUID is found.</exception>
        private void AssignMMDeviceFromEndpoint()
        {
            MMDeviceEnumerator MMDevEnumerator = new MMDeviceEnumerator();
            MMDeviceCollection MMDevCollection = MMDevEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (MMDevice MMDev in MMDevCollection)
                if (MMDev.GetGuid().Equals(Endpoint.Guid)) _MMDevice = MMDev;

            if (_MMDevice == null)
            {
                throw new NullReferenceException("Endpoint To MMDevice Mapping Exception");
            }
        }

        /// <summary>
        /// Retrieves the current capacity of the audio framebuffer associated with the endpoint.
        /// </summary>
        public int AvailableFramebufferCapacity
        {
            get
            {
                if (_AudioClient == null)
                    return 0;

                return MaxFramebufferCapacity - _AudioClient.CurrentPadding;
            }
        }

        /// <summary>
        /// Retrieves the number of bytes accepted as an audio frame by the endpoint.
        /// </summary>
        public int BytesPerFrame
        {
            get
            {
                if (_BytesPerFrame.Equals(-1))
                    _BytesPerFrame = ((_AudioClient.MixFormat.Channels * _AudioClient.MixFormat.BitsPerSample) / 8);

                return _BytesPerFrame;
            }
        }

        public void Dispose()
        {
            _AudioClient.Stop();
            State = EndpointDriverState.Unavailable;
            _AudioClient.Dispose();
            _AudioClient = null;
        }

        /// <summary>
        /// Indicates currently selected Endpoint device.
        /// </summary>
        public Endpoint Endpoint
        {
            private set
            {
                if (_Endpoint == null) 
                    return;

                if (!value.Guid.Equals(_Endpoint.Guid))
                {
                    _Endpoint = value;

                    if (!_Endpoint.Guid.Equals(_MMDevice.GetGuid()))
                        AssignMMDeviceFromEndpoint();
                }
            }

            get
            {
                return _Endpoint;
            }
        }

        /// <summary>
        /// Utalizes a MMDeviceEnumerator to fetch all available render MMDevices, converting them to Endpoint objects.
        /// </summary>
        public IEnumerable<Endpoint> GetAvailableEndpoints()
        {
            MMDeviceEnumerator MMDevEnumerator = new MMDeviceEnumerator();
            MMDeviceCollection MMDevCollection = MMDevEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            List<Endpoint>     EndpointList    = new List<Endpoint>();

            foreach (MMDevice MMDev in MMDevCollection)
                EndpointList.Add(new Endpoint(MMDev.FriendlyName, MMDev.GetGuid()));

            return EndpointList;
        }

        /// <summary>
        /// Retrieves the maximum capacity of the audio framebuffer associated with the endpoint.
        /// </summary>
        public int MaxFramebufferCapacity
        {
            get
            {
                if (_AudioClient == null)
                    return -1;

                return _AudioClient.BufferSize;
            }
        }

        /// <summary>
        /// Assigns and initializes _AudioClient. If successful, changes State property to Available. 
        /// </summary>
        private void OpenAudioClient()
        {
            _AudioClient = _MMDevice.AudioClient;

            _AudioClient.Initialize(_AudioClientShareMode, AudioClientStreamFlags.None,
                                    _BufferDuration, 0, _MMDevice.AudioClient.MixFormat, 
                                    Guid.Empty);

            _AudioClient.Start();
            State = EndpointDriverState.Available;
        }

        /// <summary>
        /// Places byte array into device memory for rendering.
        /// </summary>
        /// <param name="AudioFramebuffer">Byte array containing audio data.</param>
        /// <param name="BytesToRead">Number of bytes written to AudioFramebuffer byte array.</param>
        /// <param name="FrameCount">Number of frames contained in AudioFramebuffer byte array.</param>
        public bool RenderFramebuffer(byte[] AudioFramebuffer, int BytesToRead, int FrameCount)
        {
            if (_AudioClient != null && !BytesToRead.Equals(0))
            {
                IntPtr DeviceFramebuffer = _AudioClient.AudioRenderClient.GetBuffer(FrameCount);

                Marshal.Copy(AudioFramebuffer, 0, DeviceFramebuffer, BytesToRead);

                _AudioClient.AudioRenderClient.ReleaseBuffer(FrameCount, AudioClientBufferFlags.None);

                Thread.Sleep(25);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Assings Endpoint for audio output.
        /// </summary>
        /// <param name="Endpoint">Audio ouput endpoint.</param>
        public void SetEndpoint(Endpoint Endpoint)
        {
            Dispose();

            this.Endpoint = Endpoint;
            OpenAudioClient();
        }

        /// <summary>
        /// Indicates the availability of this IEndpointDriver instance.
        /// </summary>
        public EndpointDriverState State
        {
            private set
            {
                if (StateChangedEvent != null && value != _EndpointDriverState)
                    StateChangedEvent(this, new StateChangeEventArgs<EndpointDriverState>(_EndpointDriverState, value));

                _EndpointDriverState = value;
            }
            get
            {
                return _EndpointDriverState;
            }
        }

        /// <summary>
        /// Event to be raised when the State property changes.
        /// </summary>
        public event EventHandler<StateChangeEventArgs<EndpointDriverState>> StateChangedEvent;
    }
}
