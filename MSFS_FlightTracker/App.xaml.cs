using MSFS_FlightTracker.Properties;
using System.Windows;

namespace MSFS_FlightTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();

            base.OnExit(e);
        }
    }
}
