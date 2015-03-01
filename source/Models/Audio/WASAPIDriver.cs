using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        private int                           _BytesPerFrame        = -1;
        private Endpoint                      _Endpoint             = null;
        private EndpointDriverState           _EndpointDriverState  = EndpointDriverState.Unavailable;
        private MMDevice                      _MMDevice             = null;
        private Byte[]                        _ReadBuffer           = null;

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
                    StateChangedEvent(this, new EndpointDriverStateChangeEventArgs(State, NSE));

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
                throw new ArgumentException("Endpoint To MMDevice Mapping Exception");
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

        public void Dispose()
        {
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
        /// Assigns and initializes _AudioClient. If successful, changes State property to Available. 
        /// </summary>
        private void OpenAudioClient()
        {
            _AudioClient = _MMDevice.AudioClient;

            _AudioClient.Initialize(_AudioClientShareMode, AudioClientStreamFlags.None,
                                    (int)(_AudioClient.StreamLatency / 10000),
                                    0, _AudioClient.MixFormat, Guid.Empty);

            _BytesPerFrame = ((_AudioClient.MixFormat.Channels * _AudioClient.MixFormat.BitsPerSample) / 8);

            State = EndpointDriverState.Available;
        }

        /// <summary>
        /// Passes the AudioRenderClient buffer to an IWaveProvider to be filled and then rendered via the MMDevice.
        /// </summary>
        /// <param name="WaveProvider"></param>
        /// <param name="FrameCount"></param>
        public void ReadWaveProvider(IWaveProvider WaveProvider, int FrameCount)
        {
            if (_AudioClient == null) return;

            IntPtr Buffer     = _AudioClient.AudioRenderClient.GetBuffer(FrameCount);
            int    ReadLength = FrameCount * _BytesPerFrame;
            int    BytesRead  = WaveProvider.Read(_ReadBuffer, 0, ReadLength);

            Marshal.Copy(_ReadBuffer, 0, Buffer, BytesRead);

            _AudioClient.AudioRenderClient.ReleaseBuffer(10, AudioClientBufferFlags.None);
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
                {
                    StateChangedEvent(this, new EndpointDriverStateChangeEventArgs(value, null));
                }

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
        public event EventHandler<EndpointDriverStateChangeEventArgs> StateChangedEvent;
    }
}
