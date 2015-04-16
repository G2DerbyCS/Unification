using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// Scans for and instantiates classes from a .ddl file.
    /// </summary>
    /// <typeparam name="T">Type Specifier.</typeparam>
    internal sealed class PluginLoader<T>
    {
        /// <summary>
        /// Attempts to generate a SHA1 hash of the .dll file being read.
        /// </summary>
        /// <param name="DllPath">.dll file path.</param>
        private void ComputeDllFileHash(String DllPath)
        {
            try 
            {
                using (FileStream                FileStream            = new FileStream(DllPath, FileMode.Open))
                using (SHA1CryptoServiceProvider CryptoServiceProvider = new SHA1CryptoServiceProvider())
                {
                    DllFileHash = CryptoServiceProvider.ComputeHash(FileStream);
                }
            }
            catch (IOException)
            {
                DllFileHash = null;
            }
        }

        /// <summary>
        /// A SHA1 hash of the .dll file.
        /// </summary>
        public Byte[] DllFileHash
        {
            private set;
            get;
        }

        /// <summary>
        /// Inspects the passed .dll file for instantiable classes matching the PluginLoadder instance type specifier.
        /// </summary>
        /// <param name="Dll">Dll file to inspect.</param>
        public void LoadPluginsFrom(String DllPath)
        {
            ComputeDllFileHash(DllPath);

            foreach (Type ObjectType in Assembly.LoadFrom(DllPath).GetTypes())
            {
                if (!ObjectType.IsAbstract &&
                    !ObjectType.IsInterface &&
                    typeof(T).IsAssignableFrom(ObjectType))
                    RaiseInstanceCreatedEvent(DllPath, ObjectType);
            }
        }

        /// <summary>
        /// Event to be raised when an instance of a plugin is successfully created from the .dll file.
        /// </summary>
        public event EventHandler<PluginInstanceCreatedEventArgs<T>> OnInstanceCreatedEvent;

        /// <summary>
        /// Raises OnInstanceCreated event if OnInstanceCreatedEvent not null.
        /// </summary>
        /// <param name="Instance">New plugin instance.</param>
        private void RaiseInstanceCreatedEvent(string SourceDll, Type ObjectType)
        {
            if (OnInstanceCreatedEvent != null)
                OnInstanceCreatedEvent(this, new PluginInstanceCreatedEventArgs<T>(DllFileHash, 
                                                                                   (T)Activator.CreateInstance(ObjectType), 
                                                                                   SourceDll));
        }
    }
}
