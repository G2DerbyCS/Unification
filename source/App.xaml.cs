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
            MainWindowView _MainWindowView = new MainWindowView();
            _MainWindowView.Show();
        }

        private void WASAPIEndpointDriverTest()
        {
            System.Threading.Thread T = new System.Threading.Thread(() =>
            {
                using (Models.Audio.Interfaces.IEndpointDriver WD = new Models.Audio.WASAPIDriver())
                using (NAudio.Wave.AudioFileReader WP = new NAudio.Wave.AudioFileReader(@"")) // Insert Path To Audio FIle
                {
                    bool play = true;
                    int BytesRead = 0;
                    byte[] ReadBuffer = new byte[WD.MaxFramebufferCapacity * WD.BytesPerFrame];

                    try
                    {
                        while (play)
                        {
                            BytesRead = WP.Read(ReadBuffer, 0, (int)(WD.AvailableFramebufferCapacity * WD.BytesPerFrame));
                            play = WD.RenderFramebuffer(ReadBuffer, BytesRead, (BytesRead / WD.BytesPerFrame));
                        }

                        System.Diagnostics.Debug.WriteLine("[TEST END]");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("[EXCEPTION RAISED]\n" + ex + "\n[EXCEPTION RAISED]");
                    }
                }
            });

            T.Start();
        }
    }
}
