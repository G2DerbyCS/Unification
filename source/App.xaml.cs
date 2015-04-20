using System;
using System.Windows;
using Unification.Models.Plugins;
using Unification.Properties;

namespace Unification
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            ShutdownContentSystem();

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartupContentSystem();
        }

        private void ShutdownContentSystem()
        {
            ContentPluginMonitor.Hault();
            ContentPluginLoader.UnloadAllPluginInstances();
            ContentPluginLoader.UnWatchAllDirectories();
        }

        private void StartupContentSystem()
        {
            foreach (String DirectoryPath in Settings.Default.ContentPluginDirectorires.Split('|'))
            {
                if (!String.IsNullOrEmpty(DirectoryPath))
                    ContentPluginLoader.WatchDirectory(DirectoryPath);
            }

            ContentPluginMonitor.Init();
        }
    }
}
