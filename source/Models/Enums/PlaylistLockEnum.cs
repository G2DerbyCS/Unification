namespace Unification.Models.Enums
{
    /// <summary>
    /// An enum for use with an IPlaylist implementation for indicating criteria for IMetadataContainer selection.
    /// </summary>
    internal enum PlaylistLock
    {
        Album   = 1,
        All     = 0,
        Artist  = 3, 
        Current = 2
    }
}