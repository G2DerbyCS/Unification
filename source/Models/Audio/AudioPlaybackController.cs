using IUnification.Models.Interfaces;
using System;
using System.Threading;
using Unification.Models.Audio.Enums;
using Unification.Models.Audio.Interfaces;
using Unification.Models.Interfaces;

namespace Unification.Models.Audio
{
    /// <summary>
    /// An IPlaybackController implementation for audio track playback.
    /// </summary>
    internal class AudioPlaybackController : IPlaybackController
    {
        private Byte[]                    _AudioFrameBuffer;
        private IEndpointDriver           _EndpointDriver;
        private PlaybackState             _PlaybackState;
        private Thread                    _PlaybackThread;
        private NAudio.Wave.IWaveProvider _SampleProvider;
        private float                     _Volume;

        /// <summary>
        /// Currently loaded media.
        /// </summary>
        public IMetadataContainer Current
        {
            private set;
            get;
        }

        /// <summary>
        /// Current playlist.
        /// </summary>
        public IPlaylist DataSource
        {
            set;
            get;
        }

        /// <summary>
        /// Current auido ouput driver.
        /// </summary>
        public IEndpointDriver EndpointDriver
        {
            set
            {
                if (!PlaybackState.Equals(PlaybackState.Stopped))
                    Stop();

                if (EndpointDriver != null)
                {
                    _EndpointDriver.Dispose();
                    _EndpointDriver = null;
                }

                _EndpointDriver = value;
            }

            get
            {
                return _EndpointDriver;
            }
        }

        /// <summary>
        /// Prepares for and enteres playback loop.
        /// </summary>
        private void ExecutePlaybackLoop()
        {
            LoadPCMIEEE();
            PlaybackLoop();
        }

        /// <summary>
        /// Prepares PCM IEEE formate wave stream for rendering by the Playback loop.
        /// </summary>
        private void LoadPCMIEEE()
        {
            lock (Current)
            {
                // load logic stuff.
            }
        }

        /// <summary>
        /// Switch to next media item from DataSource.
        /// </summary>
        /// <remarks>
        /// Will start playback of next item if current PlaybackState is playing.
        /// </remarks>
        public void Next()
        {
            SwitchCurrentContainer(DataSource.GetNextContainer());
        }

        /// <summary>
        /// Pause audio playback.
        /// </summary>
        public void Pause()
        {
            PlaybackState = PlaybackState.Paused;
        }

        public void Play()
        {
            if (PlaybackState.Equals(PlaybackState.Stopped))
            {
                _PlaybackThread = new Thread(() => ExecutePlaybackLoop());
                _PlaybackThread.Start();
            }
            else if (PlaybackState.Equals(PlaybackState.Paused))
            {
                PlaybackState = PlaybackState.Playing;
            }
        }

        /// <summary>
        /// Handles progressing through current media audio data and outputting to endpoint device.
        /// </summary>
        private void PlaybackLoop()
        {
            _AudioFrameBuffer = new Byte[EndpointDriver.MaxFramebufferCapacity * EndpointDriver.BytesPerFrame];
            int BytesRead     = 0;

            while (!PlaybackState.Equals(PlaybackState.Stopped))
            {
                if (PlaybackState.Equals(PlaybackState.Paused))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    // Tweak to compensate for bytes per frame difference between sorce and endpoint.
                    // Impliment progress update.
                    // Issue STA THREAD PROBABLE.

                    BytesRead = _SampleProvider.Read(_AudioFrameBuffer,
                                                 0,
                                                 (int)(EndpointDriver.AvailableFramebufferCapacity * EndpointDriver.BytesPerFrame));

                    EndpointDriver.RenderFramebuffer(_AudioFrameBuffer, BytesRead, (BytesRead / EndpointDriver.BytesPerFrame));

                    if (BytesRead.Equals(0))
                        PlaybackState = PlaybackState.Stopped;
                }
            }

            _AudioFrameBuffer = null;
        }

        /// <summary>
        /// Audio playback state.
        /// </summary>
        public PlaybackState PlaybackState
        {
            private set
            {
                if (PlaybackStateChangedEvent != null)
                    PlaybackStateChangedEvent(this, new StateChangeEventArgs<PlaybackState>(value, _PlaybackState));

                _PlaybackState = value;
            }

            get
            {
                return _PlaybackState;
            }
        }

        public event EventHandler<StateChangeEventArgs<PlaybackState>> PlaybackStateChangedEvent;

        /// <summary>
        /// Switch to previous media item from DataSource.
        /// </summary>
        /// <remarks>
        /// Will start playback of previous item if current PlaybackState is playing.
        /// </remarks>
        public void Previous()
        {
            SwitchCurrentContainer(DataSource.GetPreviousContiner());
        }

        public IProgressIndicator ProgressIndicator
        {
            set;
            get;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Switches Current property to passed in IMetadataContainer.
        /// </summary>
        /// <param name="NewContainer">IMetadataContainer reference to assign Current property from.</param>
        private void SwitchCurrentContainer(IMetadataContainer NewContainer)
        {
            Current = NewContainer;

            switch (PlaybackState)
            {
                case PlaybackState.Paused:
                    Stop();
                    break;

                case PlaybackState.Playing:
                    Stop();
                    Play();
                    break;
            }
        }

        /// <summary>
        /// Get/Sets volume of audio output as a float value between 1 and 0.
        /// </summary>
        public float Volume
        {
            set
            {
                if (value < 0)
                {
                    _Volume = 0.0f;
                }
                else if (value > 1)
                {
                    _Volume = 1.0f;
                }
                else
                {
                    _Volume = value;
                }
            }

            get
            {
                return _Volume;
            }
        }
    }
}
