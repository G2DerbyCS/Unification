using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unification.Models.Enums;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// Scans for and instantiates types matching type specifier on List reference passed into LoadPlugins method.
    /// </summary>
    internal class PluginLoader
    {
        private Assembly _CurrentPluginAssembly;
        private String   _PluginsDirectory;

        /// <summary>
        /// Inspects PluginsDirectory .dll file assemblies for instantiable types matching types specifier,
        /// storing type instances in the Instances argument.
        /// </summary>
        /// <typeparam name="T">Type Specifier.</typeparam>
        /// <param name="Instances">Generic ICollection instance to store type instances within</param>
        public void LoadPlugins<T>(ref ICollection<T> Instances)
        {
            try
            {
                foreach (String PathToDll in Directory.EnumerateFiles(PluginsDirectory, "*.dll"))
                {
                    _CurrentPluginAssembly = Assembly.LoadFrom(PathToDll);
                    StoreTypeInstances(ref Instances);
                }

                if (LoadPluginsCompletedEvent != null)
                    LoadPluginsCompletedEvent(this, new LoadingCompletedEventArgs(LoadingState.Complete));
            }
            catch (DirectoryNotFoundException DNFEx)
            {
                if (LoadPluginsCompletedEvent != null)
                    LoadPluginsCompletedEvent(this, new LoadingCompletedEventArgs(LoadingState.Faild, DNFEx));
            }
        }

        /// <summary>
        /// An event to be raised when the loading process has been completed.
        /// </summary>
        public event EventHandler<LoadingCompletedEventArgs> LoadPluginsCompletedEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PluginLoader()
        {
            _CurrentPluginAssembly = null;
            _PluginsDirectory      = "../Plugins";
        }

        /// <summary>
        /// Sets/Gets directory to be inspected when LoadPlugins method is called.
        /// </summary>
        public String PluginsDirectory
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Plugins Directory Path Cannot be Null.");

                _PluginsDirectory = value;
            }

            get
            {
                return _PluginsDirectory;
            }
        }

        /// <summary>
        /// Reads current assembly for instantiable types matching the type specifier.
        /// </summary>
        /// <typeparam name="T">Type Specifier.</typeparam>
        /// <param name="Instances">Generic ICollection instance to store type instances within.</param>
        private void StoreTypeInstances<T>(ref ICollection<T> Instances)
        {
            if (_CurrentPluginAssembly != null)
            {
                Type[] PluginAssemblyTypes = _CurrentPluginAssembly.GetTypes();

                foreach (Type Type in PluginAssemblyTypes)
                {
                    if (Type.IsInterface || Type.IsAbstract)
                        continue;

                    if (Type.GetInterface(typeof(T).FullName) != null)
                        Instances.Add((T)Activator.CreateInstance(typeof(T)));
                }
            }
        }
    }
}