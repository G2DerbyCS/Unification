using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// Scans for and instantiates classes from .ddl files in a directory.
    /// </summary>
    /// <typeparam name="T">Type Specifier.</typeparam>
    internal sealed class PluginLoader<T>
    {
        /// <summary>
        /// Loads assemblies from .ddl files in the Directory and raises OnInstanceCreated event.
        /// </summary>
        /// <param name="Directory">Directory to scan for .dll files.</param>
        public void LoadPluginsFrom(String Directory)
        {
            foreach (String Dll in System.IO.Directory.EnumerateFiles(Directory, "*.dll"))
            {
                try
                {
                    foreach (Type ObjectType in Assembly.LoadFrom(Dll).GetTypes())
                    {
                        if (!ObjectType.IsAbstract &&
                            !ObjectType.IsInterface &&
                            typeof(T).IsAssignableFrom(ObjectType))
                            RaiseInstanceCreatedEvent(Dll, ObjectType);
                    }
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Event to be raised when an instance of a plugin is successfully created from a .dll file.
        /// </summary>
        public event EventHandler<PluginInstanceCreatedEventArgs<T>> OnInstanceCreatedEvent;

        /// <summary>
        /// Raises OnInstanceCreated event if OnInstanceCreatedEvent not null.
        /// </summary>
        /// <param name="Instance">New plugin instance.</param>
        private void RaiseInstanceCreatedEvent(string SourceDll, Type ObjectType)
        {
            if (OnInstanceCreatedEvent != null)
                OnInstanceCreatedEvent(this, new PluginInstanceCreatedEventArgs<T>(SourceDll, 
                                                                                   (T)Activator.CreateInstance(ObjectType)));
        }
    }
}
