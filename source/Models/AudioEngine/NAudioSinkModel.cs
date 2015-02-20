using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using Unification.Models.AudioEngine.Enums;

namespace Unification.Models.AudioEngine
{
    /// <summary>
    /// A singleton class that obtains access to and outputs audio to a sound device.
    /// </summary>
    internal class NAudioSink : MixingSampleProvider, IDisposable
    {
        /// <summary>
        /// Stores number of audio channels to be output to.
        /// </summary>
        private int _ChannelCount;

        /// <summary>
        /// Handles audio output to the sound device.
        /// </summary>
        private IWavePlayer _OutputDev;

        /// <summary>
        /// Stores output sample rate.
        /// </summary>
        private int _SampleRate;

        private NAudioSink(int SampleRate, int ChannelCount) : base(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, ChannelCount))
        {
            _OutputDev        = new WaveOutEvent();
            ReadFully         = true;
            this.ChannelCount = ChannelCount;
            this.SampleRate   = SampleRate;

            _OutputDev.Init(this);
            _OutputDev.Play();

            State = AudioSinkState.Available;
        }

        /// <summary>
        /// Indicates the number of audio channels to be output to.
        /// </summary>
        public int ChannelCount
        {
            private set
            {
                _ChannelCount = value;
            }

            get
            {
                return _ChannelCount;
            }
        }

        public void Dispose()
        {
            _OutputDev.Dispose();
        }

        /// <summary>
        /// Provides an instance of the AudioEngine class.
        /// </summary>
        public static readonly NAudioSink Instance = new NAudioSink(44100, 2); // Assumed Safe Defaults

        /// <summary>
        /// Indicates output sample rate.
        /// </summary>
        public int SampleRate
        {
            private set
            {
                _SampleRate = value;
            }

            get
            {
                return _SampleRate;
            }
        }

        /// <summary>
        /// Inicates the current state of this AudioEngine instance.
        /// </summary>
        public AudioSinkState State
        {
            private set;
            get;
        }
    }
}
