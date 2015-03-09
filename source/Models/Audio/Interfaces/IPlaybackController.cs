using IUnification.Models.Interfaces;
using System;
using Unification.Models.Audio.Enums;
using Unification.Models.Interfaces;

namespace Unification.Models.Audio.Interfaces
{
    /// <summary>
    /// Generic interface for all playback controllers.
    /// </summary>
    internal interface IPlaybackController
    {
        /// <summary>
        /// Indicates current IMetadataContainer.
        /// </summary>
        IMetadataContainer Current { get; }

        /// <summary>
        /// An IPlaylist implementation to provide IMetadataContainers to the IPlaybackController instance.
        /// </summary>
        IPlaylist DataSource { set; get; }

        /// <summary>
        /// The IEndpointDriver to output audio to.
        /// </summary>
        IEndpointDriver EndpointDriver { set; get; }

        /// <summary>
        /// Switches to next IMetadataContainer in DataSource.
        /// </summary>
        void Next();

        /// <summary>
        /// Pauses playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Initiates/Resumes playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Indicates IPlaybackController instance playback state.
        /// </summary>
        PlaybackState PlaybackState { get; }

        /// <summary>
        /// Event to be raised when PlaybackState property changes.
        /// </summary>
        event EventHandler<StateChangeEventArgs<PlaybackState>> PlaybackStateChangedEvent;

        /// <summary>
        /// Switches to previous IMetadataContainer in DataSource.
        /// </summary>
        void Previous();

        /// <summary>
        /// Uses IProgressIndicator instance (if not null) to indicate or adjust progress of playback.
        /// </summary>
        IProgressIndicator ProgressIndicator { set; get; }

        /// <summary>
        /// Stops playback.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets output volume.
        /// </summary>
        float Volume { set; get; }

    }
}
