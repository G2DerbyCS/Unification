using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using Unification.Models.Enums;

namespace Unification.Models
{
    /// <summary>
    /// A singleton class that obtains access to and outputs audio to a sound device.
    /// </summary>
    internal class AudioEngine : IDisposable
    {
        /// <summary>
        /// Stores number of audio channels to be output to.
        /// </summary>
        private int _ChannelCount;

        /// <summary>
        /// Handles ISampleProviders for _OutputDev.
        /// </summary>
        private MixingSampleProvider _Mixer;

        /// <summary>
        /// Handles audio output to the sound device.
        /// </summary>
        private IWavePlayer _OutputDev;

        /// <summary>
        /// Stores output sample rate.
        /// </summary>
        private int _SampleRate;

        /// <summary>
        /// Stores the current state of this AudioEngine instance.
        /// </summary>
        private AudioEngineState _State;

        private AudioEngine()
        {
            ChannelCount     = 2;     // Assumed safe default value.
            SampleRate       = 44100; // Assumed safe default value.
            _OutputDev       = new WaveOutEvent();
            _Mixer           = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, ChannelCount));
            _Mixer.ReadFully = true;

            _OutputDev.Init(_Mixer);
            _OutputDev.Play();

            State = AudioEngineState.Available;
        }

        /// <summary>
        /// Adds an ISampleProvider to the AudioEngine MixingSampleProvider for output.
        /// </summary>
        /// <param name="Sample">ISampleProvider to be added.</param>
        public void AddSampleToOutput (ISampleProvider Sample)
        {
            _Mixer.AddMixerInput(Sample);
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
        /// Event to be raised whenever a state change occurs in the audio engine.
        /// </summary>
        public event EventHandler<AudioEngineState> EngineStateChangeEvent;

        /// <summary>
        /// Provides an instance of the AudioEngine class.
        /// </summary>
        public readonly AudioEngine Instance = new AudioEngine();

        /// <summary>
        /// Removes an exsiting ISampleProvider from the AudioEngine MixingSampleProvider.
        /// </summary>
        /// <param name="Sample">ISampleProvider to be removed.</param>
        public void RemoveSampleFromOutput(ISampleProvider Sample)
        {
            _Mixer.RemoveMixerInput(Sample);
        }

        /// <summary>
        /// Indicates output sample rate.
        /// </summary>
        public int SampleRate
        {
            set
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
        public AudioEngineState State
        {
            private set
            {
                if (EngineStateChangeEvent != null)
                    EngineStateChangeEvent(this, value);

                _State = value;
            }

            get
            {
                return _State;
            }
        }
    }
}
