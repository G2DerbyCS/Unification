using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unification.Models.Plugins.Enums;
using Unification.Properties;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// Monitors and updates plugin instances provided by the ContentPluginLoader class.
    /// </summary>
    internal static class ContentPluginMonitor
    {
        #region Private Variables
        
        private static AsynchronousPState _State;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        static ContentPluginMonitor()
        {
            UpdateInterval = Settings.Default.ContentUpdateInterval;
        }

        /// <summary>
        /// Safely terminates all internal asynchronous activity ending plugin monitoring.
        /// </summary>
        public static void Hault()
        {
            State = AsynchronousPState.Halted;
        }

        /// <summary>
        /// Initializes content plugin monitoring.
        /// </summary>
        public static void Init()
        {
            if (PluginUpdateThread == null)
                PluginUpdateThread = new Thread(() => UpdatePluginMetadataCollections());

            State = AsynchronousPState.Idling;
            PluginUpdateThread.Start();
        }

        /// <summary>
        /// Thread on which to execute plugin metadatacollections updates and plugin monitoring.
        /// </summary>
        private static Thread PluginUpdateThread;

        /// <summary>
        /// Indicates the process state of internal asynchronous activity.
        /// </summary>
        /// <remarks>
        /// Setting value to AsynchronousPState.Halted will terminate all internal asynchronous activity and
        /// require a that Init() be called in order to resume.
        /// </remarks>
        public static AsynchronousPState State
        {
            private set
            {
                lock (ThreadLockObj)
                    _State = value;
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
        private static readonly Object ThreadLockObj = new Object();

        /// <summary>
        /// Indicates the time interval between updating the plugin's metadatacollections.
        /// </summary>
        public static TimeSpan UpdateInterval;

        /// <summary>
        /// Itterates through plugin instances calling their UpdateMetadataCollection method
        /// and updating their PluginInstanceWrapper(IUnification) container properties.
        /// </summary>
        /// <remarks>
        /// Utilizes Prallel.Foreach to improve perforamce.
        /// </remarks>
        private static void UpdatePluginMetadataCollections()
        {
            while (!State.Equals(AsynchronousPState.Halted))
            {
                State = AsynchronousPState.Processing;

                Parallel.ForEach(ContentPluginLoader.EnumerateContentPluginInstanceWrappers, (InstanceWrapper, LoopState) =>
                {
                    try
                    {
                        InstanceWrapper.Instance.UpdateMetadataCollection();
                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine("\n[ContentPluginMonitor : UpdatePluginMetadataCollections]");
                        Debug.WriteLine(Ex + "\n");
                    }
                    finally
                    {
                        if (!State.Equals(AsynchronousPState.Processing))
                            LoopState.Break();
                    }
                });

                if (!State.Equals(AsynchronousPState.Halted))
                {
                    State = AsynchronousPState.Idling;

                    Thread.Sleep(UpdateInterval);
                }
            }
        }
    }
}