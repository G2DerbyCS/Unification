using IUnification.Models.Interfaces;
using System;
using System.Collections.Generic;
using Unification.Models.Enums;
using Unification.Models.Interfaces;

namespace Unification.Models
{
    /// <summary>
    /// Basic IPlaylist class [COMPLETELY UNTESTED].
    /// </summary>
    internal sealed class MetadataPlaylist : IPlaylist
    {
        #region private_variables
        int                      _CurrentIndex;
        List<IMetadataContainer> _PlaylistContent;
        List<int>                _PreviousContainerIndexes;
        bool                     _UsingPreviousIndexs;
        #endregion

        /// <summary>
        /// Defines criteria for selection of IMetadataContainers returned by GetNextContiner method.
        /// </summary>
        public PlaylistLock FetchLock
        {
            set
            {
                throw new NotImplementedException();
            }

            get
            {
                return PlaylistLock.All;
            }
        }

        /// <summary>
        /// Determines the index of the IMetadataContainer in _PlaylistContent to return to the GetNextContainer method.
        /// </summary>
        /// <returns>_PlaylistContent index.</returns>
        private int GenerateNextIndex()
        {
            if (_PlaylistContent.Count.Equals(_PreviousContainerIndexes.Count))
            {
                if (LoopMode.Equals(PlaylistLoopMode.End))
                {
                    return -1;
                }
                else
                {
                    _PreviousContainerIndexes.Clear();
                    _PreviousContainerIndexes.TrimExcess();

                    _CurrentIndex = 0;
                }
            }

            if (SequenceMode.Equals(PlaylistSequenceMode.Random))
            {
                Random rng = new Random();

                if (_PreviousContainerIndexes.Count.Equals(0))
                {
                    _CurrentIndex = rng.Next(0, _PlaylistContent.Count);
                }
                else
                {
                    while (_PreviousContainerIndexes.Contains(_CurrentIndex))
                        _CurrentIndex = rng.Next(0, _PlaylistContent.Count);
                }
            }
            else
            {
                _CurrentIndex++;
            }

            _PreviousContainerIndexes.Add(_CurrentIndex);

            return _CurrentIndex;
        }

        /// <summary>
        /// Retrieves next IMetadataContainer.
        /// </summary>
        public IMetadataContainer GetNextContainer()
        {
            if (_PlaylistContent == null)
                return null;

            try
            {
                if (_UsingPreviousIndexs)
                {
                    return _PlaylistContent[ReturnIndexFromPrevious()];
                }
                else
                {
                    return _PlaylistContent[GenerateNextIndex()];
                }
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns previous IMetadataContainer.
        /// </summary>
        public IMetadataContainer GetPreviousContiner()
        {
            if (_PlaylistContent == null)
                return null;

            if (_PreviousContainerIndexes.Count.Equals(0))
                return GetNextContainer();

            if (_UsingPreviousIndexs)
            {
                if (!_CurrentIndex.Equals(0))
                    _CurrentIndex--;

                return _PlaylistContent[_PreviousContainerIndexes[_CurrentIndex]];
            }
            else
            {
                _CurrentIndex        = _PreviousContainerIndexes.Count - 1;
                _UsingPreviousIndexs = true;

                return _PlaylistContent[_PreviousContainerIndexes[_CurrentIndex]];
            }
        }

        /// <summary>
        /// Defines IPlaylist instance PlaylistContent end-reached action.
        /// </summary>
        public PlaylistLoopMode LoopMode
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MetadataPlaylist()
        {
            _CurrentIndex             = 0;
            _PreviousContainerIndexes = new List<int>();
            _UsingPreviousIndexs      = false;
        }

        /// <summary>
        /// Returns the index of a previously returned IMetadataContainer to the GetNextContainer method.
        /// </summary>
        /// <returns>_PlaylistContent index.</returns>
        private int ReturnIndexFromPrevious()
        {
            if (_CurrentIndex.Equals(_PreviousContainerIndexes.Count - 1))
            {
                _CurrentIndex        = _PreviousContainerIndexes[_CurrentIndex];
                _UsingPreviousIndexs = false;

                return GenerateNextIndex();
            }
            else
            {
                _CurrentIndex++;

                return _PreviousContainerIndexes[_CurrentIndex];
            }
        }

        /// <summary>
        /// Defines the order in which IMetadataContainers are selected by GetNextContainer method.
        /// </summary>
        public PlaylistSequenceMode SequenceMode
        {
            get;
            set;
        }

        // <summary>
        /// Assigns a list of IMetadataContainers to serve as the content for this IPlaylist instance.
        /// </summary>
        public void SetPlaylistContent(ref List<IMetadataContainer> PlaylistContent)
        {
            _PlaylistContent = PlaylistContent;
            _CurrentIndex    = 0;

            _PreviousContainerIndexes.Clear();
            _PreviousContainerIndexes.TrimExcess();
        }
    }
}
