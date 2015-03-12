using System.Windows;
using Unification.Views;

namespace Unification
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Thread T;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindowView _MainWindowView = new MainWindowView();
            _MainWindowView.Show();

            _MainWindowView.Closing += (sv, ve) =>
                {
                    T.Abort();
                };

            WASAPIEndpointDriverPlayThreadInit();
            T.Start();
        }

        private void WASAPIEndpointDriverPlayThreadInit()
        {
            T = new System.Threading.Thread(() =>
            {
                using (Models.Audio.Interfaces.IEndpointDriver WD = new Models.Audio.WASAPIDriver())
                using (NAudio.Wave.AudioFileReader WP = new NAudio.Wave.AudioFileReader(@"../../TestFiles/Feint - Snake Eyes [Lyrics].mp3")) // Insert Path To Audio FIle
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
        }
    }
}
