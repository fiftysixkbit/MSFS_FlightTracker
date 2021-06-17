using MapControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
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
    public partial class MainWindow : Window
    {
        private int _maxDownload = -1;
        private SimvarsViewModel simvarVm;
        private double? lastLatitude;
        private double? lastLongitude;

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
            simvarVm.SetOnTickCallback(new BaseCommand((p) => { SimvarOnTick(); }));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CenterOnLatLong(44.837788, -0.579180);
        }

        private void SimvarOnTick()
        {
            try
            {
                var latitude = simvarVm.bLatitude;
                var longitude = simvarVm.bLongitude;
                var heading = simvarVm.bHeading;

                UpdateStatuses(simvarVm.bConnected, simvarVm.bTrackingStarted);

                // Update plane marker
                UpdatePlaneMarker(heading);

                if (simvarVm.bFollowMap)
                {
                    CenterOnLatLong(latitude, longitude, this.tileCanvas.Zoom);
                }

                if (simvarVm.bTrackingStarted)
                {
                    // Draw circle marker
                    // Only draw if the plane is moving
                    if (lastLatitude != null && lastLongitude != null)
                    {
                        if (lastLatitude != latitude || lastLongitude != longitude)
                        {
                            DrawCircle(latitude, longitude);
                        }
                    }
                    else
                    {
                        DrawCircle(latitude, longitude);
                    }

                    // No use for these yet, was thinking of using it to join lines
                    lastLatitude = latitude;
                    lastLongitude = longitude;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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

        public void DrawCircle(double latitude, double longitude)
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
        }

        public void RemoveAllCircles()
        {
            tileCanvas.Children.RemoveRange(3, tileCanvas.Children.Count);
        }

        public void UpdateStatuses(bool connected, bool tracking)
        {
            if (connected)
            {
                connectedStatusLabel.Content = "YES";
                connectedStatusLabel.Foreground = Brushes.Green;
            }
            else
            {
                connectedStatusLabel.Content = "NO";
                connectedStatusLabel.Foreground = Brushes.Red;
            }

            if (tracking)
            {
                trackingStatusLabel.Content = "YES";
                trackingStatusLabel.Foreground = Brushes.Green;
            }
            else
            {
                trackingStatusLabel.Content = "NO";
                trackingStatusLabel.Foreground = Brushes.Red;
            }
        }

        public void CenterOnLatLong(double degLat, double degLong, int zoom = 12)
        {
            this.tileCanvas.Focus();
            this.tileCanvas.Center(degLat, degLong, zoom);
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

        public void UpdatePlaneMarker(double heading)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();

            if (heading >= 5 && heading < 15)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_010.png");
            }
            else if (heading >= 15 && heading < 25)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_020.png");
            }
            else if (heading >= 25 && heading < 35)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_030.png");
            }
            else if (heading >= 35 && heading < 45)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_040.png");
            }
            else if (heading >= 45 && heading < 55)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_050.png");
            }
            else if (heading >= 55 && heading < 65)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_060.png");
            }
            else if (heading >= 65 && heading < 75)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_070.png");
            }
            else if (heading >= 75 && heading < 85)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_080.png");
            }
            else if (heading >= 85 && heading < 95)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_090.png");
            }
            else if (heading >= 95 && heading < 105)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_100.png");
            }
            else if (heading >= 155 && heading < 115)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_110.png");
            }
            else if (heading >= 115 && heading < 125)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_120.png");
            }
            else if (heading >= 125 && heading < 135)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_130.png");
            }
            else if (heading >= 135 && heading < 145)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_140.png");
            }
            else if (heading >= 145 && heading < 155)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_150.png");
            }
            else if (heading >= 155 && heading < 165)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_160.png");
            }
            else if (heading >= 165 && heading < 175)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_170.png");
            }
            else if (heading >= 175 && heading < 185)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_180.png");
            }
            else if (heading >= 185 && heading < 195)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_190.png");
            }
            else if (heading >= 195 && heading < 255)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_200.png");
            }
            else if (heading >= 255 && heading < 215)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_210.png");
            }
            else if (heading >= 215 && heading < 225)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_220.png");
            }
            else if (heading >= 225 && heading < 235)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_230.png");
            }
            else if (heading >= 235 && heading < 245)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_240.png");
            }
            else if (heading >= 245 && heading < 255)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_250.png");
            }
            else if (heading >= 255 && heading < 265)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_260.png");
            }
            else if (heading >= 265 && heading < 275)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_270.png");
            }
            else if (heading >= 275 && heading < 285)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_280.png");
            }
            else if (heading >= 285 && heading < 295)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_290.png");
            }
            else if (heading >= 295 && heading < 305)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_300.png");
            }
            else if (heading >= 355 && heading < 315)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_310.png");
            }
            else if (heading >= 315 && heading < 325)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_320.png");
            }
            else if (heading >= 325 && heading < 335)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_330.png");
            }
            else if (heading >= 335 && heading < 345)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_340.png");
            }
            else if (heading >= 345 && heading < 355)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_350.png");
            }
            else if (heading >= 355 || heading < 5)
            {
                image.UriSource = new Uri("pack://application:,,,/MSFS_FlightTracker;component/images/ic_airplane_360.png");
            }

            image.EndInit();

            planeMarker.Source = image;
        }
    }
}
