using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using MapControl;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSFS_FlightTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private int _maxDownload = -1;
        private SimvarsViewModel simvarVm;
        private double? lastLatitude;
        private double? lastLongitude;
        public const int CHART_TICK_INTERVAL = 2;

        public MainWindow()
        {
            // Very important we set the CacheFolder before doing anything so the MapCanvas knows where
            // to save the downloaded files to.
            TileGenerator.CacheFolder = @"ImageCache";
            TileGenerator.UserAgent = "MSFS_FlightTracker";
            TileGenerator.DownloadCountChanged += this.OnDownloadCountChanged;
            TileGenerator.DownloadError += this.OnDownloadError;

            this.DataContext = new SimvarsViewModel(this);
            
            InitializeComponent();
            CommandManager.AddPreviewExecutedHandler(this, this.PreviewExecuteCommand); // We're going to do some effects when zooming.

            simvarVm = (SimvarsViewModel)DataContext;
            simvarVm.SetOnTickCallback(new BaseCommand(async (p) => { await SimvarOnTickAsync(p); }));

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CenterOnLatLong(44.837788, -0.579180);
        }

        private async Task<Task> SimvarOnTickAsync(object param)
        {
            try
            {
                var index = (int) param;

                var latitude = simvarVm.bLatitude;
                var longitude = simvarVm.bLongitude;
                var heading = simvarVm.bHeading;
                var planeAltitude = simvarVm.bPlaneAltitude;
                var groundAltitude = simvarVm.bGroundAltitude;

                // Update plane marker
                await UpdatePlaneMarker(heading);

                if (simvarVm.bFollowMap)
                {
                    await CenterOnLatLong (latitude, longitude, tileCanvas.Zoom);
                }

                if (simvarVm.bTrackingStarted)
                {
                    // Draw circle marker
                    // Only draw if the plane is moving
                    if (lastLatitude != null && lastLongitude != null)
                    {
                        if (lastLatitude != latitude || lastLongitude != longitude)
                        {
                            await DrawCircle(latitude, longitude);
                            if (index % CHART_TICK_INTERVAL == 0)
                            {
                                await UpdateCharts();
                            }
                        }
                    }
                    else
                    {
                        await DrawCircle (latitude, longitude);
                        await UpdateCharts();
                    }

                    // No use for these yet, was thinking of using it to join lines
                    lastLatitude = latitude;
                    lastLongitude = longitude;

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                simvarVm.lErrorMessages.Add("SimvarOnTick: " + ex.ToString());
            }

            return Task.CompletedTask;
        }

        protected HwndSource GetHWinSource()
        {
            return PresentationSource.FromVisual(this) as HwndSource;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            GetHWinSource().AddHook(WndProc);
            if (this.DataContext is IBaseSimConnectWrapper oBaseSimConnectWrapper)
            {
                oBaseSimConnectWrapper.SetWindowHandle(GetHWinSource().Handle);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled)
        {
            if (this.DataContext is IBaseSimConnectWrapper oBaseSimConnectWrapper)
            {
                try
                {
                    if (iMsg == oBaseSimConnectWrapper.GetUserSimConnectWinEvent())
                    {
                        oBaseSimConnectWrapper.ReceiveSimConnectMessage();
                    }
                }
                catch
                {
                    oBaseSimConnectWrapper.Disconnect();
                }
            }

            return IntPtr.Zero;
        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri); // Launch the site in the user's default browser.
        }

        private void OnDownloadCountChanged(object sender, EventArgs e)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.OnDownloadCountChanged(sender, e)), null);
                return;
            }
            if (TileGenerator.DownloadCount == 0)
            {
                this.label.Visibility = Visibility.Hidden;
                this.progress.Visibility = Visibility.Hidden;
                _maxDownload = -1;
            }
            else
            {
                this.errorBar.Visibility = Visibility.Collapsed;

                if (_maxDownload < TileGenerator.DownloadCount)
                {
                    _maxDownload = TileGenerator.DownloadCount;
                }
                this.progress.Value = 100 - (TileGenerator.DownloadCount * 100.0 / _maxDownload);
                this.progress.Visibility = Visibility.Visible;
                this.label.Text = string.Format(
                    CultureInfo.CurrentUICulture,
                    "Downloading {0} item{1}",
                    TileGenerator.DownloadCount,
                    TileGenerator.DownloadCount != 1 ? 's' : ' ');
                this.label.Visibility = Visibility.Visible;
            }
        }

        private void OnDownloadError(object sender, EventArgs e)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.OnDownloadError(sender, e)), null);
                return;
            }

            this.errorBar.Text = "Unable to contact the server to download map data.";
            this.errorBar.Visibility = Visibility.Visible;
        }

        private Task UpdateCharts()
        {
            foreach (var series in simvarVm.AltitudeSeries)
            {
                if (series.Title == "Plane Altitude")
                {
                    series.Values.Add(simvarVm.bPlaneAltitude);
                }
                else if (series.Title == "Ground Altitude")
                {
                    series.Values.Add(simvarVm.bGroundAltitude);
                }
                else if (series.Title == "Water")
                {
                    series.Values.Add(0.0);
                }
            }

            foreach (var series in simvarVm.SpeedSeries)
            {
                if (series.Title == "True Airspeed")
                {
                    series.Values.Add(simvarVm.bAirspeedTrue);
                }
                else if (series.Title == "Indicated Airspeed")
                {
                    series.Values.Add(simvarVm.bAirspeedIndicated);
                }
                else if (series.Title == "Ground Speed")
                {
                    series.Values.Add(simvarVm.bGroundSpeed);
                }
            }

            return Task.CompletedTask;
        }

        public Task DrawCircle(double latitude, double longitude)
        {
            var circle = new Ellipse();
            circle.Width = 5;
            circle.Height = 5;

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
            circle.Fill = mySolidColorBrush;

            circle.SetValue(MapCanvas.LatitudeProperty, latitude);
            circle.SetValue(MapCanvas.LongitudeProperty, longitude);

            this.tileCanvas.Children.Add(circle);

            return Task.CompletedTask;
        }

        public void RemoveAllCircles()
        {
            tileCanvas.Children.RemoveRange(3, tileCanvas.Children.Count);
        }

        public Task CenterOnLatLong(double degLat, double degLong, int zoom = 12)
        {
            this.tileCanvas.Focus();
            this.tileCanvas.Center(degLat, degLong, zoom);

            return Task.CompletedTask;
        }

        private void OnZoomStoryboardCompleted(object sender, EventArgs e)
        {
            this.zoomGrid.Visibility = Visibility.Hidden;
            this.zoomImage.Source = null;
        }

        private void PreviewExecuteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == NavigationCommands.DecreaseZoom)
            {
                if (this.tileCanvas.Zoom > 0) // Make sure we can actualy zoom out
                {
                    this.StartZoom("zoomOut", 1);
                }
            }
            else if (e.Command == NavigationCommands.IncreaseZoom)
            {
                if (this.tileCanvas.Zoom < TileGenerator.MaxZoom)
                {
                    this.StartZoom("zoomIn", 0.5);
                }
            }
        }

        private void StartZoom(string name, double scale)
        {
            this.zoomImage.Source = this.tileCanvas.CreateImage();
            this.zoomRectangle.Height = this.tileCanvas.ActualHeight * scale;
            this.zoomRectangle.Width = this.tileCanvas.ActualWidth * scale;

            this.zoomGrid.RenderTransform = new ScaleTransform(); // Clear the old transform
            this.zoomGrid.Visibility = Visibility.Visible;
            ((Storyboard)this.zoomGrid.FindResource(name)).Begin();
        }

        public Task UpdatePlaneMarker(double heading)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();

            if (heading >= 2.5 && heading < 7.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_005.png");
            }
            if (heading >= 7.5 && heading < 12.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_010.png");
            }
            else if (heading >= 12.5 && heading < 17.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_015.png");
            }
            else if (heading >= 17.5 && heading < 22.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_020.png");
            }
            else if (heading >= 22.5 && heading < 27.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_025.png");
            }
            else if (heading >= 27.5 && heading < 32.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_030.png");
            }
            else if (heading >= 32.5 && heading < 37.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_035.png");
            }
            else if (heading >= 37.5 && heading < 42.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_040.png");
            }
            else if (heading >= 42.5 && heading < 47.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_045.png");
            }
            else if (heading >= 47.5 && heading < 52.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_050.png");
            }
            else if (heading >= 52.5 && heading < 57.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_055.png");
            }
            else if (heading >= 57.5 && heading < 62.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_060.png");
            }
            else if (heading >= 62.5 && heading < 67.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_065.png");
            }
            else if (heading >= 67.5 && heading < 72.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_070.png");
            }
            else if (heading >= 72.5 && heading < 77.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_075.png");
            }
            else if (heading >= 77.5 && heading < 82.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_080.png");
            }
            else if (heading >= 82.5 && heading < 87.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_085.png");
            }
            else if (heading >= 87.5 && heading < 92.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_090.png");
            }
            else if (heading >= 92.5 && heading < 97.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_095.png");
            }
            else if (heading >= 97.5 && heading < 102.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_100.png");
            }
            else if (heading >= 102.5 && heading < 107.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_105.png");
            }
            else if (heading >= 107.5 && heading < 112.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_110.png");
            }
            else if (heading >= 112.5 && heading < 117.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_115.png");
            }
            else if (heading >= 117.5 && heading < 122.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_120.png");
            }
            else if (heading >= 122.5 && heading < 127.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_125.png");
            }
            else if (heading >= 127.5 && heading < 132.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_130.png");
            }
            else if (heading >= 132.5 && heading < 137.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_135.png");
            }
            else if (heading >= 137.5 && heading < 142.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_140.png");
            }
            else if (heading >= 142.5 && heading < 147.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_145.png");
            }
            else if (heading >= 147.5 && heading < 152.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_150.png");
            }
            else if (heading >= 152.5 && heading < 157.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_155.png");
            }
            else if (heading >= 157.5 && heading < 162.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_160.png");
            }
            else if (heading >= 162.5 && heading < 167.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_165.png");
            }
            else if (heading >= 167.5 && heading < 172.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_170.png");
            }
            else if (heading >= 172.5 && heading < 177.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_175.png");
            }
            else if (heading >= 177.5 && heading < 182.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_180.png");
            }
            else if (heading >= 182.5 && heading < 187.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_185.png");
            }
            else if (heading >= 187.5 && heading < 192.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_190.png");
            }
            else if (heading >= 192.5 && heading < 197.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_195.png");
            }
            else if (heading >= 197.5 && heading < 202.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_200.png");
            }
            else if (heading >= 202.5 && heading < 207.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_205.png");
            }
            else if (heading >= 207.5 && heading < 212.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_210.png");
            }
            else if (heading >= 212.5 && heading < 217.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_215.png");
            }
            else if (heading >= 217.5 && heading < 222.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_220.png");
            }
            else if (heading >= 222.5 && heading < 227.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_225.png");
            }
            else if (heading >= 227.5 && heading < 232.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_230.png");
            }
            else if (heading >= 232.5 && heading < 237.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_235.png");
            }
            else if (heading >= 237.5 && heading < 242.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_240.png");
            }
            else if (heading >= 242.5 && heading < 247.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_245.png");
            }
            else if (heading >= 247.5 && heading < 252.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_250.png");
            }
            else if (heading >= 252.5 && heading < 257.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_255.png");
            }
            else if (heading >= 257.5 && heading < 262.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_260.png");
            }
            else if (heading >= 262.5 && heading < 267.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_265.png");
            }
            else if (heading >= 267.5 && heading < 272.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_270.png");
            }
            else if (heading >= 272.5 && heading < 277.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_275.png");
            }
            else if (heading >= 277.5 && heading < 282.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_280.png");
            }
            else if (heading >= 282.5 && heading < 287.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_285.png");
            }
            else if (heading >= 287.5 && heading < 292.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_290.png");
            }
            else if (heading >= 292.5 && heading < 297.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_295.png");
            }
            else if (heading >= 297.5 && heading < 302.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_300.png");
            }
            else if (heading >= 302.5 && heading < 307.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_305.png");
            }
            else if (heading >= 307.5 && heading < 312.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_310.png");
            }
            else if (heading >= 312.5 && heading < 317.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_315.png");
            }
            else if (heading >= 317.5 && heading < 322.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_320.png");
            }
            else if (heading >= 322.5 && heading < 327.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_325.png");
            }
            else if (heading >= 327.5 && heading < 332.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_330.png");
            }
            else if (heading >= 332.5 && heading < 337.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_335.png");
            }
            else if (heading >= 337.5 && heading < 342.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_340.png");
            }
            else if (heading >= 342.5 && heading < 347.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_345.png");
            }
            else if (heading >= 347.5 && heading < 352.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_350.png");
            }
            else if (heading >= 352.5 && heading < 357.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_355.png");
            }
            else if (heading >= 357.5 || heading < 2.5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_360.png");
            }

            image.EndInit();

            planeMarker.Source = image;

            return Task.CompletedTask;
        }

        private void tileCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            simvarVm.bFollowMap = false;
        }

        private void ShowPlaneAltitudeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (simvarVm != null && simvarVm.AltitudeSeries != null)
            {
                foreach (GLineSeries series in simvarVm.AltitudeSeries)
                {
                    if (series.Title == "Plane Altitude")
                    {
                        series.Visibility = simvarVm.bShowPlaneAltitude ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        private void ShowGroundAltitudeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (simvarVm != null && simvarVm.AltitudeSeries != null)
            {
                foreach (GLineSeries series in simvarVm.AltitudeSeries)
                {
                    if (series.Title == "Ground Altitude")
                    {
                        series.Visibility = simvarVm.bShowGroundAltitude ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        private void ShowWaterAltitudeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (simvarVm != null && simvarVm.AltitudeSeries != null)
            {
                foreach (GLineSeries series in simvarVm.AltitudeSeries)
                {
                    if (series.Title == "Water")
                    {
                        series.Visibility = simvarVm.bShowWaterAltitude ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        private void ShowAirspeedTrueCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (simvarVm != null && simvarVm.SpeedSeries != null)
            {
                foreach (GLineSeries series in simvarVm.SpeedSeries)
                {
                    if (series.Title == "True Airspeed")
                    {
                        series.Visibility = simvarVm.bShowAirspeedTrue ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        private void ShowAirspeedIndicatedCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (simvarVm != null && simvarVm.SpeedSeries != null)
            {
                foreach (GLineSeries series in simvarVm.SpeedSeries)
                {
                    if (series.Title == "Indicated Airspeed")
                    {
                        series.Visibility = simvarVm.bShowAirspeedIndicated ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        private void ShowGroundSpeedCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (simvarVm != null && simvarVm.SpeedSeries != null)
            {
                foreach (GLineSeries series in simvarVm.SpeedSeries)
                {
                    if (series.Title == "Ground Speed")
                    {
                        series.Visibility = simvarVm.bShowGroundSpeed ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }
    }
}
