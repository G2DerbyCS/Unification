using System;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// An EventArgs class for indicating instantiation of an object from a .dll file.
    /// </summary>
    /// <typeparam name="T">Type Specifier.</typeparam>
    internal sealed class PluginInstanceCreatedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Object instance.
        /// </summary>
        public readonly T Instance;

        /// <summary>
        /// Path to the .dll file object instance was loaded from.
        /// </summary>
        public readonly String SourceDll;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="SourceDll">Path to .dll file.</param>
        /// <param name="Instance">Object instance.</param>
        public PluginInstanceCreatedEventArgs(String SourceDll, T Instance)
        {
            this.Instance  = Instance;
            this.SourceDll = SourceDll;
        }
    }
}
