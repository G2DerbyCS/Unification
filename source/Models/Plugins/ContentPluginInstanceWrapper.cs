using IUnification.Models.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// A wrapper class for use in monitoring and storing unification content plugin state information. 
    /// </summary>
    internal sealed class ContentPluginInstanceWrapper : IDisposable
    {
        /// <summary>
        /// Attatches IUnification Instance to this ContentPluginInstanceWrapper instance.
        /// </summary>
        /// <remarks>
        /// Will perform deserialization of any stored information if necessary.
        /// </remarks>
        /// <param name="Instances"></param>
        private void AttatchToPluginInstance()
        {
            if (Instance is IConfigurableUnificationPlugin)
                DeserializePluginConfiguration();

            Instance.MetadataCollectionUpdatedEvent += StoreCollectionUpdateEventArguments;
        }

        /// <summary>
        /// Initializes instance from information provided by a PluginInstanceCreatedEventArgs(IUnificationPlugin) instance.
        /// </summary>
        /// <param name="CreationEventArgs">PluginInstanceCreatedEventArgs(IUnificationPlugin) instance.</param>
        public ContentPluginInstanceWrapper(PluginInstanceCreatedEventArgs<IUnificationPlugin> CreationEventArgs) :
            this (CreationEventArgs.DllFileHash, CreationEventArgs.Instance, CreationEventArgs.SourceDll)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Instance">IUnificationPlugin implementation instance.</param>
        /// <param name="SourceDll">The path to the .dll file the instance was created from.</param>
        public ContentPluginInstanceWrapper(Byte[] DllFileHash, IUnificationPlugin Instance, String SourceDll)
        {
            this.DllFileHash = DllFileHash;
            this.Instance    = Instance;
            this.SourceDll   = SourceDll;

            AttatchToPluginInstance();
        }

        /// <summary>
        /// Deserializes IConfigurableUnificationPlugin properties from disk if possible.
        /// </summary>
        private void DeserializePluginConfiguration()
        {
            System.Diagnostics.Debug.WriteLine("\n[ContentPluginInstanceWrapper : DeserializePluginConfiguration]");
            System.Diagnostics.Debug.WriteLine("Deserialization unimplemented\n");
        }

        /// <summary>
        /// Performes resource cleanup and calls dispose on the Instance property.
        /// </summary>
        public void Dispose()
        {
            if (Instance is IConfigurableUnificationPlugin)
                SerializePluginConfiguration();

            Instance.Dispose();
        }

        /// <summary>
        /// A SHA1 hash of the source .dll file.
        /// </summary>
        public readonly Byte[] DllFileHash;

        /// <summary>
        /// The IUnification implementation instance monitored by this ContentPluginINstanceWrapper instance.
        /// </summary>
        public readonly IUnificationPlugin Instance;

        /// <summary>
        /// Retains the the event arguments from the last time the instance's MetadataCollectionUpdatedEvent event occured.
        /// </summary>
        public IUnification.Models.LoadingCompletedEventArgs LastLoadResult
        {
            private set;
            get;
        }

        /// <summary>
        /// Serializes IConfigurableUnificationPlugin properties to disk.
        /// </summary>
        private void SerializePluginConfiguration()
        {
            System.Diagnostics.Debug.WriteLine("\n[ContentPluginInstanceWrapper : DeserializePluginConfiguration]");
            System.Diagnostics.Debug.WriteLine("Serialization unimplemented \n");
        }

        /// <summary>
        /// The .dll file the Instance property was created from.
        /// </summary>
        public readonly String SourceDll;

        /// <summary>
        /// Assigns the value of the LastLoadResult property when the MetadataCollectionUpdatedEvent occures on the Instance 
        /// property.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoreCollectionUpdateEventArguments(object sender, IUnification.Models.LoadingCompletedEventArgs e)
        {
            LastLoadResult = e;
        }
    }
}