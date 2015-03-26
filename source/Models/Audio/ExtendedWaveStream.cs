using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace Unification.Models.Audio
{
    /// <summary>
    /// Lighter weight impliementation of NAudio's NAudio.Wave.SampleProviders.SampleChannel class.
    /// ExtendedWaveStream class takes in a WaveStream instance input at any bit depth and exposes it
    /// as both an IWaveProvider and an ISampleProvider; Also exposing seeking (if input WaveStream)
    /// allows seeking and providing a volume mechanism.
    /// </summary>
    internal sealed class ExtendedWaveStream : IWaveProvider, ISampleProvider, IDisposable
    {
        private readonly WaveStream           _BaseWaveStream;
        private readonly VolumeSampleProvider _VolumeSampleProvider;
        private readonly object               _ThreadLockObj;

        /// <summary>
        /// Disposes of the base WaveStream object.
        /// </summary>
        public void Dispose()
        {
            if (_BaseWaveStream != null)
                _BaseWaveStream.Dispose();
        }

        /// <summary>
        /// Gets/Sets the current position in the stream in Time format.
        /// </summary>
        /// <remarks>
        /// Conditions for InvalidCastException:
        ///     WaveStream object is COM object.
        ///     WaveStream object was created on STA thread.
        ///     Accessing thread is not the thread the WaveStream object was created on.
        /// 
        /// Solution  for InvalidCastException:
        ///     Simply eating the exception allows the class to continue exhibiting 
        ///     expected behaviour.
        /// </remarks>
        public TimeSpan CurrentTime
        {
            set
            {
                try
                {
                    if (_BaseWaveStream.CanSeek)
                        _BaseWaveStream.CurrentTime = value;
                }
                catch (InvalidCastException)
                { }
            }

            get
            {
                return _BaseWaveStream.CurrentTime;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="BaseWaveStreamObj">WaveStream instance to extend.</param>
        public ExtendedWaveStream(WaveStream BaseWaveStreamObj)
        {
            if (BaseWaveStreamObj == null)
                throw new ArgumentNullException("BaseWaveStreamObj Cannot be Set to NULL");

            if (BaseWaveStreamObj.CanRead.Equals(false))
                throw new ArgumentException("Unsupported WaveStream Object.", "WaveStream Object Cannot be Read.");

            _BaseWaveStream       = BaseWaveStreamObj;
            _VolumeSampleProvider = new VolumeSampleProvider(BaseWaveStreamObj.ToSampleProvider());
            _ThreadLockObj        = new object();
        }

        /// <summary>
        /// Reads from this wave stream
        /// </summary>
        /// <param name="buffer">Audio buffer</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">Number of bytes required</param>
        /// <returns>Number of bytes read</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            WaveBuffer WaveBuffer      = new WaveBuffer(buffer);
            int        RequiredSamples = count / 4;
            int        SamplesRead     = Read(WaveBuffer.FloatBuffer, offset / 4, RequiredSamples);

            return (SamplesRead * 4);
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (_ThreadLockObj)
                  return _VolumeSampleProvider.Read(buffer, offset, count);
        }

        /// <summary>
        /// Total length in real-time of the stream (may be an estimate for compressed files).
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return _BaseWaveStream.TotalTime;
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

                _VolumeSampleProvider.Volume = value;
            }

            get
            {
                return _VolumeSampleProvider.Volume;
            }
        }

        /// <summary>
        /// Retrieves Wave format.
        /// </summary>
        public WaveFormat WaveFormat
        {
            get 
            {
                return _VolumeSampleProvider.WaveFormat;
            }
        }
    }
}
