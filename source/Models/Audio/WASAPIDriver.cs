using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unification.Models.Audio.Enums;
using Unification.Models.Audio.Interfaces;

namespace Unification.Models.Audio
{
    internal class WASAPIDriver : IEndpointDriver
    {
        private AudioClient                   _AudioClient          = null;
        private readonly AudioClientShareMode _AudioClientShareMode = AudioClientShareMode.Shared;
        private int                           _BytesPerFrame        = -1;
        private Endpoint                      _Endpoint             = null;
        private EndpointDriverState           _EndpointDriverState  = EndpointDriverState.Unavailable;
        private MMDevice                      _MMDevice             = null;
        private Byte[]                        _ReadBuffer           = null;

        public WASAPIDriver() : this(null)
        { }

        public WASAPIDriver(Endpoint Endpoint)
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                State = EndpointDriverState.Unsupported;
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

        private void AssignDefaultEndpoint()
        {
            _MMDevice = (new MMDeviceEnumerator()).GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _Endpoint = new Endpoint(_MMDevice.FriendlyName, _MMDevice.GetGuid());
        }

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

        public IEnumerable<Endpoint> AvailableEndpoints
        {
            private set;
            get;
        }

        public void Dispose()
        {
            _AudioClient.Dispose();
            _AudioClient = null;
        }

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

        private void OpenAudioClient()
        {
            _AudioClient = _MMDevice.AudioClient;

            _AudioClient.Initialize(_AudioClientShareMode, AudioClientStreamFlags.None,
                                    (int)(_AudioClient.StreamLatency / 10000),
                                    0, _AudioClient.MixFormat, Guid.Empty);

            _BytesPerFrame = ((_AudioClient.MixFormat.Channels * _AudioClient.MixFormat.BitsPerSample) / 8);

            State = EndpointDriverState.Available;
        }

        public void ParseBuffer(IWaveProvider WaveProvider, int FrameCount)
        {
            if (_AudioClient == null) return;

            IntPtr Buffer     = _AudioClient.AudioRenderClient.GetBuffer(FrameCount);
            int    ReadLength = FrameCount * _BytesPerFrame;
            int    BytesRead  = WaveProvider.Read(_ReadBuffer, 0, ReadLength);

            Marshal.Copy(_ReadBuffer, 0, Buffer, BytesRead);

            _AudioClient.AudioRenderClient.ReleaseBuffer(10, AudioClientBufferFlags.None);
        }

        public void SetEndpoint(Endpoint Endpoint)
        {
            throw new NotImplementedException();
        }

        public EndpointDriverState State
        {
            private set
            {
                if (StateChangedEvent != null)
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

        public event EventHandler<EndpointDriverStateChangeEventArgs> StateChangedEvent;
    }
}
