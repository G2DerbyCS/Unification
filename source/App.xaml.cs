using System.Windows;
using Unification.Views;

namespace Unification
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            (new MainWindowView()).Show();
        }
    }
}
