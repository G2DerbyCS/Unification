using IUnification.Models.Enums;
using IUnification.Models.Interfaces;
using System;
using System.IO;
using System.Threading;
using Unification.Models.Audio.Enums;
using Unification.Models.Audio.Interfaces;
using Unification.Models.Interfaces;

namespace Unification.Models.Audio
{
    /// <summary>
    /// An IPlaybackController implementation for audio track playback.
    /// </summary>
    internal sealed class NAudioPlaybackController : IAudioPlaybackController
    {
        private Byte[]               _AudioFrameBuffer;
        private IAudioEndpointDriver _EndpointDriver;
        private PlaybackState        _PlaybackState;
        private Thread               _PlaybackThread;
        private IProgressIndicator   _ProgressIndicator;
        private float                _Volume;
        private ExtendedWaveStream   _WaveStream;
        private object               _WaveStreamObjectLock = new object();

        /// <summary>
        /// Currently loaded media.
        /// </summary>
        public IMetadataContainer Current
        {
            private set;
            get;
        }

        /// <summary>
        /// Disposes of internal resources and the EndpointDriver currently assosiated with this NAudioPlaybackController.
        /// </summary>
        public void Dispose()
        {
            lock (_WaveStreamObjectLock)
            {
                if (_WaveStream != null)
                {
                    _WaveStream.Dispose();
                    _WaveStream = null;
                }
            }

            if (EndpointDriver != null)
            {
                EndpointDriver.Dispose();
                EndpointDriver = null;
            }
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
        public IAudioEndpointDriver EndpointDriver
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
            lock (_WaveStreamObjectLock)
            {
                lock (Current)
                    CreateWaveStream();

                if (ProgressIndicator != null)
                    OnProgressIndicatorUpdateEvent(this, new StateChangeEventArgs<float>(ProgressIndicator.Progress));

                PlaybackLoop();
            }
        }

        /// <summary>
        /// Prepares PCM IEEE formate wave stream for rendering by the Playback loop.
        /// </summary>
        private void CreateWaveStream()
        {
            NAudio.Wave.WaveStream BaseWaveStreamObj = null;

            switch(Current.DatasourceFormat)
            {
                case DatasourceFormat.Local:
                    string FilePath = Current.Datasource.ToString();

                    switch (Path.GetExtension(FilePath))
                    {
                        case ".aiff":
                            BaseWaveStreamObj = new NAudio.Wave.AiffFileReader(FilePath);
                            break;

                        case ".mp3":
                            BaseWaveStreamObj = new NAudio.Wave.Mp3FileReader(FilePath);
                            break;

                        case ".wav":
                            BaseWaveStreamObj = new NAudio.Wave.WaveFileReader(FilePath);

                            if (!BaseWaveStreamObj.WaveFormat.Encoding.Equals(NAudio.Wave.WaveFormatEncoding.Pcm) &&
                                !BaseWaveStreamObj.WaveFormat.Encoding.Equals(NAudio.Wave.WaveFormatEncoding.IeeeFloat))
                            {
                                BaseWaveStreamObj = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(BaseWaveStreamObj);
                                BaseWaveStreamObj = new NAudio.Wave.BlockAlignReductionStream(BaseWaveStreamObj);
                            }
                            break;

                        default:
                            try
                            {
                                BaseWaveStreamObj = new NAudio.Wave.MediaFoundationReader(Current.Datasource.LocalPath);
                            }
                            catch (Exception Ex)
                            {
                                throw new NotImplementedException("File Format is Currently Unsupported.", Ex);
                            }
                            break;
                    }
                    break;

                default:
                    throw new ArgumentException("Unsupported DatasourceFormat : " + Current.DatasourceFormat);
            }

            if (BaseWaveStreamObj != null)
            {
                if (BaseWaveStreamObj.WaveFormat.Channels.Equals(EndpointDriver.ChannelCount) &&
                    BaseWaveStreamObj.WaveFormat.SampleRate.Equals(EndpointDriver.SampleRate))
                {
                    _WaveStream = new ExtendedWaveStream(BaseWaveStreamObj);
                }
                else
                {
                    try
                    {
                        _WaveStream = new ExtendedWaveStream(new NAudio.Wave.ResamplerDmoStream(BaseWaveStreamObj, 
                                                                                                new NAudio.Wave.WaveFormat(EndpointDriver.SampleRate, EndpointDriver.ChannelCount)));
                    }
                    catch(Exception)
                    {
                        throw new NotImplementedException("Decompressed Audio Data Format Incompatible With Endpoint.");
                    }
                }

                SetWaveStreamVolume();
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

        /// <summary>
        /// Begins or continues audio playback.
        /// </summary>
        public void Play()
        {
            if (PlaybackState.Equals(PlaybackState.Stopped))
            {
                if (EndpointDriver == null)
                    throw new NullReferenceException("Endpoint Driver is NULL or unset.");

                if (Current != null)
                {
                    _PlaybackThread = new Thread(() => ExecutePlaybackLoop());
                    _PlaybackThread.Start();
                }
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
            int BytesToRead   = 0;
            PlaybackState     = PlaybackState.Playing;

            while (!PlaybackState.Equals(PlaybackState.Stopped))
            {
                if (PlaybackState.Equals(PlaybackState.Paused))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    try
                    {
                        BytesToRead = (int)(EndpointDriver.AvailableFramebufferCapacity * EndpointDriver.BytesPerFrame);
                        BytesRead   = _WaveStream.Read(_AudioFrameBuffer, 0, BytesToRead);
                        
                        EndpointDriver.RenderFramebuffer(_AudioFrameBuffer, BytesRead, (BytesRead / EndpointDriver.BytesPerFrame));
                    }
                    catch (Exception Ex)
                    {
                        if (PlaybackStateChangedEvent != null)
                            PlaybackStateChangedEvent(this, new StateChangeEventArgs<PlaybackState>(PlaybackState.Stopped, PlaybackState, Ex));

                        _PlaybackState = PlaybackState.Stopped;
                    }

                    UpdateProgressIndicator();

                    if (BytesRead.Equals(0))
                        Next();
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

        /// <summary>
        /// Event to be raised when the PlaybackState property changes.
        /// </summary>
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

        /// <summary>
        /// Uses IProgressIndicator instance (if not null) to indicate or adjust progress of playback.
        /// </summary>
        public IProgressIndicator ProgressIndicator
        {
            set
            {
                _ProgressIndicator = value;

                if (_ProgressIndicator != null)
                    _ProgressIndicator.ProgressUpdatedEvent += OnProgressIndicatorUpdateEvent;
            }

            get
            {
                return _ProgressIndicator;
            }
        }

        /// <summary>
        /// Updates seek position when ProgressIndicator's Progress property changes.
        /// </summary>
        /// <param name="sender">Object raising ProgressIndicator's ProgressUpdatedEvent event.</param>
        /// <param name="e">State changed event arguments.</param>
        private void OnProgressIndicatorUpdateEvent(object sender, StateChangeEventArgs<float> e)
        {
            if (_WaveStream != null)
            {
                if (!_WaveStream.CurrentTime.Equals(TimeSpan.FromSeconds(_WaveStream.TotalTime.TotalSeconds * e.CurrentState)))
                    _WaveStream.CurrentTime = TimeSpan.FromSeconds(_WaveStream.TotalTime.TotalSeconds * e.CurrentState);
            }
        }

        /// <summary>
        /// Sets the _WaveStream object's volume to the value of the Volume property.
        /// </summary>
        private void SetWaveStreamVolume()
        {
            if (_WaveStream != null)
                _WaveStream.Volume = Volume;
        }

        /// <summary>
        /// Stops audio playback and disposes wave stream object.
        /// </summary>
        public void Stop()
        {
            PlaybackState = PlaybackState.Stopped;

            lock (_WaveStreamObjectLock)
            {
                if (_WaveStream != null)
                {
                    _WaveStream.Dispose();
                    _WaveStream = null;
                }
            }

            if (ProgressIndicator != null)
                ProgressIndicator.Progress = 0.0f;
        }

        /// <summary>
        /// Switches Current property to passed in IMetadataContainer.
        /// </summary>
        /// <param name="NewContainer">IMetadataContainer to assign Current property from.</param>
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
        /// An array containing officially supported file formats.
        /// </summary>
        /// <remarks>
        /// NAudioPlaybackController will attempt to decode "unsupported" formats using the NAudio.Wave.MediaFoundationReader class.
        /// </remarks>
        private static string[] SupportedFF = new string[] 
        {
            ".aiff",
            ".mp3",
            ".wav"
        };

        /// <summary>
        /// Returns an array containing all file formats that can be handled by an NAudioPlaybackController instance.
        /// </summary>
        public string[] SupportedFileFormats
        {
            get
            {
                return SupportedFF;
            }
        }

        /// <summary>
        /// Updates progress indicator on current thread.
        /// </summary>
        /// <remarks>
        /// Conditions for ArgumentException:
        ///     _WaveStream.CurrentTime.Ticks sometimes returns a larger value than _WaveStream.TotalTime.Ticks
        ///     
        /// Solution for ArgumentException:
        ///     Switching to the next track and continuing playback allows the class to continue exhibiting 
        ///     expected behaviour.
        /// </remarks>
        private void UpdateProgressIndicator()
        {
            if (_WaveStream != null && ProgressIndicator != null)
            {
                try
                {
                    ProgressIndicator.Progress = _WaveStream.CurrentTime.Ticks / (float)_WaveStream.TotalTime.Ticks;
                }
                catch (ArgumentException)
                {
                    Next();
                }
            }
        }

        /// <summary>
        /// Get/Sets volume of audio output as a float value between 1 and 0.
        /// </summary>
        public float Volume
        {
            set
            {
                if (value > 1.0f || value < 0.0f)
                    throw new ArgumentException("Voume must be a float between 1 and 0");

                _Volume = value;

                SetWaveStreamVolume();
            }

            get
            {
                return _Volume;
            }
        }
    }
}
