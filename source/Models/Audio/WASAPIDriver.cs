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
    internal sealed class WASAPIDriver : IAudioEndpointDriver
    {
        private AudioClient                   _AudioClient          = null;
        private readonly AudioClientShareMode _AudioClientShareMode = AudioClientShareMode.Shared;
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
                if (StateChangedEvent != null)
                    StateChangedEvent(this, 
                                      new StateChangeEventArgs<EndpointDriverState>(State, 
                                                                                    EndpointDriverState.Unavailable, 
                                                                                    new NotSupportedException("WASAPI supported only on Windows Vista and above")));

                _EndpointDriverState = EndpointDriverState.Unsupported;
            }
            else
            {
                this.Endpoint = Endpoint;

                if (this.Endpoint == null)
                    AssignDefaultEndpoint();

                OpenAudioClient();
            }
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
                throw new NullReferenceException("Endpoint To MMDevice Mapping Exception");
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
            private set;
            get;
        }

        /// <summary>
        /// Returns the number of audio channels available (1=Mono, 2=Stereo, etc).
        /// </summary>
        public int ChannelCount
        {
            get
            {
                return _AudioClient.MixFormat.Channels;
            }
        }

        public void Dispose()
        {
            State = EndpointDriverState.Unavailable;

            if (_AudioClient != null)
            {
                _AudioClient.Stop();
                _AudioClient.Dispose();
                _AudioClient = null;
            }
        }

        /// <summary>
        /// Indicates currently selected Endpoint device.
        /// </summary>
        public Endpoint Endpoint
        {
            private set
            {
                if (_Endpoint == null)
                {
                    _Endpoint = value;
                    AssignMMDeviceFromEndpoint();
                }
                else if (!_Endpoint.Guid.Equals(value.Guid))
                {
                    _Endpoint = value;
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
            _AudioClient  = _MMDevice.AudioClient;
            BytesPerFrame = ((_AudioClient.MixFormat.Channels * _AudioClient.MixFormat.BitsPerSample) / 8);

            _AudioClient.Initialize(_AudioClientShareMode, AudioClientStreamFlags.None,
                                    (_MMDevice.AudioClient.MixFormat.SampleRate * 10), 
                                    0, _MMDevice.AudioClient.MixFormat, Guid.Empty);

            _AudioClient.Start();

            State = EndpointDriverState.Available;
        }

        /// <summary>
        /// Places byte array into device memory for rendering.
        /// </summary>
        /// <param name="AudioFramebuffer">Byte array containing audio data.</param>
        /// <param name="BytesToRead">Number of bytes written to AudioFramebuffer byte array.</param>
        /// <param name="FrameCount">Number of frames contained in AudioFramebuffer byte array.</param>
        public void RenderFramebuffer(byte[] AudioFramebuffer, int BytesToRead, int FrameCount)
        {
            if (_AudioClient != null && !BytesToRead.Equals(0))
            {
                IntPtr DeviceFramebuffer = _AudioClient.AudioRenderClient.GetBuffer(FrameCount);
                
                Marshal.Copy(AudioFramebuffer, 0, DeviceFramebuffer, BytesToRead);

                _AudioClient.AudioRenderClient.ReleaseBuffer(FrameCount, AudioClientBufferFlags.None);

                Thread.Sleep(15);
            }
        }

        /// <summary>
        /// Retrieves the sample rate (in Hertz) that the endpoint is currently functioning at.
        /// </summary>
        public int SampleRate
        {
            get
            {
                if (_AudioClient == null)
                    return -1;

                return _AudioClient.MixFormat.SampleRate;
            }
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
