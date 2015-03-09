using IUnification.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using Unification.Models.Enums;

namespace Unification.Models.Interfaces
{
    /// <summary>
    /// Generic interface for all Playlists.
    /// </summary>
    internal interface IPlaylist : INotifyPropertyChanged
    {
        /// <summary>
        /// Defines IPlaylist instance PlaylistContent end-reached action.
        /// </summary>
        PlaylistLoopMode LoopMode { set; get; }

        /// <summary>
        /// Defines criteria for selection of IMetadataContainers returned by GetNextContiner method.
        /// </summary>
        PlaylistLock FetchLock { set; get; }

        /// <summary>
        /// Returns previous IMetadataContainer.
        /// </summary>
        /// <returns></returns>
        IMetadataContainer GetPreviousContiner();

        /// <summary>
        /// Retrieves next IMetadataContainer.
        /// </summary>
        /// <returns></returns>
        IMetadataContainer GetNextContainer();

        /// <summary>
        /// Defines the order in which IMetadataContainers are selected by GetNextContainer method.
        /// </summary>
        PlaylistSequenceMode SequenceMode { set; get; }

        /// <summary>
        /// Assigns a list of IMetadataContainers to serve as the content for this IPlaylist instance.
        /// </summary>
        /// <param name="PlaylistContent">IMetadataContainer list.</param>
        void SetPlaylistContent (ref IList<IMetadataContainer> PlaylistContent);
    }
}
