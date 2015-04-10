using System;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// A wrapper class for a PluginInstanceCreatedEventArgs class instance, 
    /// that allows for storage of plugin instance state information.
    /// </summary>
    /// <typeparam name="T">Type Specifier.</typeparam>
    internal sealed class PluginInstanceWrapper<T>
    {
        /// <summary>
        /// Plugin Instance.
        /// </summary>
        public T Instance
        {
            get
            {
                return PluginInstanceEventArgs.Instance;
            }
        }

        /// <summary>
        /// Contains the exception raised during the last attempt at calling the plugin's UpdateMetadataCollection method.
        /// </summary>
        /// <remarks>
        /// Will be null if no exception was raised.
        /// </remarks>
        public Exception LoadException
        {
            set;
            get;
        }

        /// <summary>
        /// Holds the outcome of the last attempt at calling the plugin's UpdateMetadataCollection method.
        /// </summary>
        /// <remarks>
        /// Will be null if the plugin's UpdateMetadataCollection method has yet to be called.
        /// </remarks>
        public IUnification.Models.LoadingCompletedEventArgs LastLoadResult
        {
            set;
            get;
        }

        /// <summary>
        /// PluginInstanceCreatedEventArgs class instance.
        /// </summary>
        private readonly PluginInstanceCreatedEventArgs<T> PluginInstanceEventArgs;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="PluginInstanceEventArgs">Class instance to wrap.</param>
        public PluginInstanceWrapper(PluginInstanceCreatedEventArgs<T> PluginInstanceEventArgs)
        {
            LastLoadResult               = null;
            this.PluginInstanceEventArgs = PluginInstanceEventArgs;
        }

        /// <summary>
        /// Path to .dll file.
        /// </summary>
        public String SourceDll
        {
            get
            {
                return PluginInstanceEventArgs.SourceDll;
            }
        }
    }
}
