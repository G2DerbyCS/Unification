using IUnification.Models.Enums;
using IUnification.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Unification.Models.Plugins.Enums;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// A class designed to manage unification content plugins and 
    /// provide access to their combined metadata collections. 
    /// </summary>
    internal sealed class UnificationPluginManager : IDisposable, INotifyPropertyChanged
    {
        #region Private Reference Variables

        private String             _PluginsDirectory = String.Empty;
        private AsynchronousPState _State            = AsynchronousPState.Halted;
        private TimeSpan           _UpdateInterval   = TimeSpan.FromSeconds(600);

        #endregion

        /// <summary>
        /// Assigns a value to a property and raises the PropertyChanged event if not null.
        /// </summary>
        /// <typeparam name="T">Type Specifier.</typeparam>
        /// <param name="Property">Reference to property to assign Value to.</param>
        /// <param name="PropertyName">Property name/title for PropertyChanged event call.</param>
        /// <param name="Value">Value to be assigned to referenced property.</param>
        private void AssignPropertyValue<T>(ref T Property, String PropertyName, T Value)
        {
            Property = Value;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Thread on which to execute plugin metadatacollections updates and plugin monitoring.
        /// </summary>
        private readonly Thread AsyncFunctionsThread;

        /// <summary>
        /// A composite collection of all the MetadataCollections presented by the loaded IUnification instances.
        /// </summary>
        public readonly CompositeCollection CompositeMetadataCollection;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UnificationPluginManager(String DirectoryPath)
        {
            AsyncFunctionsThread                =  new Thread(() => MonitorPlugins());
            CompositeMetadataCollection         =  new CompositeCollection();
            PluginInstances                     =  new List<PluginInstanceWrapper<IUnificationPlugin>>();
            PluginLoader                        =  new PluginLoader<IUnificationPlugin>();
            PluginLoader.OnInstanceCreatedEvent += OnPluginInstanceCreatedEvent;
            PluginsDirectory                    =  DirectoryPath;
            State                               =  AsynchronousPState.Processing;

            AsyncFunctionsThread.Start();
        }

        /// <summary>
        /// Disposes of resources heald by this instance.
        /// </summary>
        public void Dispose()
        {
            State = AsynchronousPState.Halted;
            AsyncFunctionsThread.Join();
            CompositeMetadataCollection.Clear();
            DisposePlugins();
        }

        /// <summary>
        /// Disposes plugin instances in PluginInstances list and 
        /// calls Clear and TrimExcess methods on PluginInstances list.
        /// </summary>
        private void DisposePlugins()
        {
            for (int i = 0; i < PluginInstances.Count; i++)
            {
                PluginInstances[i].Dispose();
                PluginInstances[i] = null;
            }

            PluginInstances.Clear();
            PluginInstances.TrimExcess();
        }

        /// <summary>
        /// Monitors plugin instances and updates their MetadataCollections.
        /// </summary>
        private void MonitorPlugins()
        {
            PluginLoader.LoadPluginsFrom(PluginsDirectory);

            if (!PluginInstances.Count.Equals(0))
            {
                while (!State.Equals(AsynchronousPState.Halted))
                {
                    Parallel.ForEach(PluginInstances, (PInstance, LoopState) =>
                    {
                        try
                        {
                            PInstance.Instance.UpdateMetadataCollection();
                            PInstance.Instance.MetadataCollectionUpdatedEvent += (
                                (se, ev) =>
                                {
                                    PInstance.LastLoadResult = ev;
                                    PInstance.LoadException  = null;
                                });
                        }
                        catch (Exception Ex)
                        {
                            PInstance.LastLoadResult = new IUnification.Models.LoadingCompletedEventArgs(LoadingState.Faild);
                            PInstance.LoadException  = Ex;
                        }

                        if (State.Equals(AsynchronousPState.Halted))
                            LoopState.Break();
                    });

                    lock (ThreadLockObj)
                    {
                        if (!State.Equals(AsynchronousPState.Halted))
                        {
                            State = AsynchronousPState.Idling;

                            Thread.Sleep(UpdateInterval);

                            State = AsynchronousPState.Processing;
                        }
                    }
                }
            }

            if (!State.Equals(AsynchronousPState.Halted))
                State = AsynchronousPState.Halted;
        }

        /// <summary>
        /// Directory to inspect for plugins.
        /// </summary>
        public String PluginsDirectory
        {
            private set
            {
                if (!_PluginsDirectory.Equals(value))
                    AssignPropertyValue(ref _PluginsDirectory, "PluginsDirectory", value);
            }

            get
            {
                return _PluginsDirectory;
            }
        }

        /// <summary>
        /// Places new IUnificationPlugin instances in the Plugins list and correct sub-lists.
        /// </summary>
        /// <param name="sender">Object sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPluginInstanceCreatedEvent(object sender, PluginInstanceCreatedEventArgs<IUnificationPlugin> e)
        {
            PluginInstances.Add(new PluginInstanceWrapper<IUnificationPlugin>(e));
            CompositeMetadataCollection.Add(e.Instance.MetadataCollection);
        }

        /// <summary>
        /// Event to be raised when a unification content plugin instance is created.
        /// </summary>
        public event EventHandler<PluginInstanceCreatedEventArgs<IUnificationPlugin>> PluginInstanceCreatedEvent
        {
            add
            {
                PluginLoader.OnInstanceCreatedEvent += value;
            }

            remove
            {
                PluginLoader.OnInstanceCreatedEvent -= value;
            }
        }

        /// <summary>
        /// List of all instantiated plugin instance wrappers.
        /// </summary>
        public readonly List<PluginInstanceWrapper<IUnificationPlugin>> PluginInstances;

        /// <summary>
        /// Plugin Loader. 
        /// </summary>
        private readonly PluginLoader<IUnificationPlugin> PluginLoader;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates the process state of the update and loading thread.
        /// </summary>
        public AsynchronousPState State
        {
            private set
            {
                lock (ThreadLockObj)
                    AssignPropertyValue(ref _State, "State", value);
            }

            get
            {
                lock (ThreadLockObj)
                    return _State;
            }
        }

        /// <summary>
        /// Arbitary object for use in lock method structures.
        /// </summary>
        private readonly Object ThreadLockObj = new Object();

        /// <summary>
        /// Indicates the time interval between updating the plugin's metadatacollections.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            set
            {
                lock (ThreadLockObj)
                    AssignPropertyValue(ref _UpdateInterval, "UpdateInterval", value);
            }

            get
            {
                lock (ThreadLockObj)
                    return _UpdateInterval;
            }
        }
    }
}
