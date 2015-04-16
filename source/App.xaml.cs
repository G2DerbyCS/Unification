using System.Windows;
using Unification.Models.Plugins;

namespace Unification
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            ContentPluginMonitor.Hault();
            ContentPluginLoader.UnloadAllPluginInstances();
            ContentPluginLoader.UnWatchAllDirectories();

            base.OnExit(e);
        }
    }
}
