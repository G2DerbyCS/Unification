using IUnification.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// Monitors folders instantiating concrete IUnification interface implementations from .dll files.
    /// </summary>
    /// <remarks>
    /// ThreadSafe.
    /// Provides a CompositeCollection of all plugin MetadataCollection ObservableCollections.
    /// </remarks>
    internal static class ContentPluginLoader
    {
        #region Private Variables

        private static List<ContentPluginInstanceWrapper> _ContentPluginInstanceWrappers;
        private static CompositeCollection                _CompositeMetadataCollection;

        #endregion
        /// <summary>
        /// List of all available ContentPluginInstanceWrapper instances.
        /// </summary>
        private static List<ContentPluginInstanceWrapper> ContentPluginInstanceWrappers
        {
            set
            {
                lock (ThreadLockObj)
                    _ContentPluginInstanceWrappers = value;
            }
            get
            {
                lock (ThreadLockObj)
                    return _ContentPluginInstanceWrappers;
            }
        }

        /// <summary>
        /// A composite collection of all the MetadataCollections presented by the loaded IUnification instances.
        /// </summary>
        public static CompositeCollection CompositeMetadataCollection
        {
            private set
            {
                lock (ThreadLockObj)
                    _CompositeMetadataCollection = value;
            }

            get
            {
                lock (ThreadLockObj)
                    return _CompositeMetadataCollection;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        static ContentPluginLoader()
        {
            ContentPluginInstanceWrappers =  new List<ContentPluginInstanceWrapper>();
            CompositeMetadataCollection   =  new CompositeCollection();
            FileSystemWatchers            =  new List<FileSystemWatcher>();
            PluginLoader                  =  new PluginLoader<IUnificationPlugin>();
            PluginInstanceCreatedEvent    += OnContentPluginInstanceCreatedEvent;
        }

        /// <summary>
        /// Enumerates all available ContentPluginInstanceWrapper instances.
        /// </summary>
        public static IEnumerable<ContentPluginInstanceWrapper> EnumerateContentPluginInstanceWrappers
        {
            get
            {
                return ContentPluginInstanceWrappers;
            }
        }

        /// <summary>
        /// Enumerates all directories with an attached FileSystemWatcher instance.
        /// </summary>
        public static IEnumerable<String> EnumerateWatchedDirectories
        {
            get
            {
                foreach (FileSystemWatcher FileSystemWatcher in FileSystemWatchers) 
                        yield return FileSystemWatcher.Path;
            }
        }

        /// <summary>
        /// Retrieves FileSystemWatcher from FileSystemWatchers based on path.
        /// </summary>
        /// <param name="Path">Path to watched directory.</param>
        /// <returns></returns>
        private static FileSystemWatcher FetchFileSystemWatcher(String Path)
        {
            foreach (FileSystemWatcher FileSystemWatcher in FileSystemWatchers)
            {
                if (FileSystemWatcher.Path.Equals(Path))
                    return FileSystemWatcher;
            }

            return null;
        }

        /// <summary>
        /// Method to be executed when a FileSystemWatcher raises its Created event.
        /// </summary>
        /// <param name="sender">Object raising event.</param>
        /// <param name="e">Event arguments.</param>
        private static void FileSystemWatcher_CreatedEvent(object sender, FileSystemEventArgs e)
        {
            PluginLoader.LoadPluginsFrom(e.FullPath);
        }

        /// <summary>
        /// List of active FileSystemWatcher instances.
        /// </summary>
        private static readonly List<FileSystemWatcher> FileSystemWatchers;

        /// <summary>
        /// Inspects directory for .dll files from which to load content plugin instances.
        /// </summary>
        /// <param name="DirectoryPath">Directory to inspect for .dll files.</param>
        private static void InspectDllFiles(String DirectoryPath)
        {
            foreach (String DllPath in Directory.EnumerateFiles(DirectoryPath, "*.dll"))
            {
                try
                {
                    PluginLoader.LoadPluginsFrom(DllPath);
                }
                catch(FileLoadException)
                { }
            }
        }

        /// <summary>
        /// Inspects ContentPluginInstanceWrappers list and verifies that the PluginInstanceCreatedEventArgs instance is 
        /// unique and not already initialized.
        /// </summary>
        /// <param name="e">Plugin creation event arguments.</param>
        /// <returns>If plugin instance is unique.</returns>
        private static bool IsUnique(PluginInstanceCreatedEventArgs<IUnificationPlugin> e)
        {
            return !ContentPluginInstanceWrappers.Any(
                InstanceWrapper => InstanceWrapper.DllFileHash.SequenceEqual(e.DllFileHash)  &&
                                   InstanceWrapper.Instance.Title.Equals(e.Instance.Title)   &&
                                   InstanceWrapper.Instance.Author.Equals(e.Instance.Author) &&
                                   InstanceWrapper.Instance.Version.Equals(e.Instance.Version));
        }                          

        /// <summary>
        /// Method to be executed when the PluginLoader raises its OnInstanceCreatedEvent event.
        /// </summary>
        /// <param name="sender">Object raising event.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnContentPluginInstanceCreatedEvent(object sender, 
                                                                PluginInstanceCreatedEventArgs<IUnificationPlugin> e)
        {
            if (IsUnique(e))
            {
                CollectionContainer CollectionContainer = new CollectionContainer();
                CollectionContainer.Collection          = e.Instance.MetadataCollection;

                CompositeMetadataCollection.Add(CollectionContainer);
                ContentPluginInstanceWrappers.Add(new ContentPluginInstanceWrapper(e));
            }
            else
            {
                e.Instance.Dispose();
            }
        }

        /// <summary>
        /// Event to be raised when a unification content plugin instance is created.
        /// </summary>
        public static event EventHandler<PluginInstanceCreatedEventArgs<IUnificationPlugin>> PluginInstanceCreatedEvent
        {
            add    { PluginLoader.OnInstanceCreatedEvent += value; }
            remove { PluginLoader.OnInstanceCreatedEvent -= value; }
        }

        /// <summary>
        /// Plugin loader.
        /// </summary>
        private static readonly PluginLoader<IUnificationPlugin> PluginLoader;

        /// <summary>
        /// Arbitrary object for use in lock() structures.
        /// </summary>
        private static readonly Object ThreadLockObj = new Object();

        /// <summary>
        /// Unloads all content plugin instances via their ContentPluginInstanceWrapper.
        /// </summary>
        public static void UnloadAllPluginInstances()
        {
            foreach (ContentPluginInstanceWrapper ContentPluginInstanceWrapper in ContentPluginInstanceWrappers)
            {
                ContentPluginInstanceWrapper.Dispose();
            }

            ContentPluginInstanceWrappers.Clear();
            ContentPluginInstanceWrappers.TrimExcess();
            CompositeMetadataCollection.Clear();
        }

        /// <summary>
        /// Unloads a plugin instance via it's ContentPluginInstanceWrapper.
        /// </summary>
        /// <param name="PluginInstanceWrapper"></param>
        public static void UnloadPluginInstance(ContentPluginInstanceWrapper PluginInstanceWrapper)
        {
            if (ContentPluginInstanceWrappers.Contains(PluginInstanceWrapper))
            {
                foreach (CollectionContainer CollectionContainer in CompositeMetadataCollection)
                {
                    if (CollectionContainer.Collection.Equals(PluginInstanceWrapper.Instance.MetadataCollection))
                        CompositeMetadataCollection.Remove(CollectionContainer);
                }

                PluginInstanceWrapper.Dispose();
                ContentPluginInstanceWrappers.Remove(PluginInstanceWrapper);
            }
        }

        /// <summary>
        /// Removes all directories from monitoring.
        /// </summary>
        public static void UnWatchAllDirectories()
        {
            foreach (FileSystemWatcher FileSystemWatcher in FileSystemWatchers)
            {
                FileSystemWatcher.Dispose();
            }

            FileSystemWatchers.Clear();
            FileSystemWatchers.TrimExcess();
        }

        /// <summary>
        /// Removes a directory from monitoring.
        /// </summary>
        /// <param name="DirectoryPath"></param>
        public static void UnwatchDirectory(String DirectoryPath)
        {
            FileSystemWatcher FileSystemWatcher = FetchFileSystemWatcher(DirectoryPath);

            if (FileSystemWatcher != null)
            {
                FileSystemWatcher.Dispose();
                FileSystemWatchers.Remove(FileSystemWatcher);
            }
        }

        /// <summary>
        /// Adds a directory for monitoring.
        /// </summary>
        public static void WatchDirectory(String DirectoryPath)
        {
            FileSystemWatcher FileSystemWatcher = FetchFileSystemWatcher(DirectoryPath);

            if (FileSystemWatcher == null)
            {
                FileSystemWatcher                     =  new FileSystemWatcher();
                FileSystemWatcher.Path                =  DirectoryPath;
                FileSystemWatcher.NotifyFilter        =  NotifyFilters.FileName;
                FileSystemWatcher.Filter              =  "*.dll";
                FileSystemWatcher.Created             += FileSystemWatcher_CreatedEvent;
                FileSystemWatcher.EnableRaisingEvents =  true;

                FileSystemWatchers.Add(FileSystemWatcher);
                InspectDllFiles(DirectoryPath);
            }
        }
    }
}
