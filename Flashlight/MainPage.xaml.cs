using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;
using System.Reflection;
using System.Threading;

namespace Flashlight
{
    public partial class MainPage : ContentPage
    {
        private double width;
        private double height;
        public MainPage()
        {
            InitializeComponent();

            ApplyMapTheme();
        }

        private void ApplyMapTheme()
        {
            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"Flashlight.MapResources.MapTheme.json");
            string themeFile;
            using(var reader = new System.IO.StreamReader(stream))
            {
                themeFile = reader.ReadToEnd();
                map.MapStyle = MapStyle.FromJson(themeFile);
            }
        }

        private async void FlashlightOnButtonOnClicked(object sender, EventArgs e)
        {
            try
            {
                await Xamarin.Essentials.Flashlight.TurnOnAsync();

                powerOnBtn.IsVisible = true;
                powerOffBtn.IsVisible = false;
                BackgroundColor = Color.FromHex("#152232");
                DisplayCurrentLocation();
            }
            catch
            {
                powerOnBtn.IsVisible = false;
                powerOffBtn.IsVisible = true;
                map.IsVisible = false;
                BackgroundColor = Color.Black;
            }
        }

        private async void FlashlightOnButtonOffClicked(object sender, EventArgs e)
        {
            try
            {
                await Xamarin.Essentials.Flashlight.TurnOffAsync();

                powerOnBtn.IsVisible = false;
                powerOffBtn.IsVisible = true;
                map.IsVisible = false;
                BackgroundColor = Color.Black;
            }
            catch
            {
                powerOnBtn.IsVisible = true;
                powerOffBtn.IsVisible = false;
                BackgroundColor = Color.FromHex("#152232");
                DisplayCurrentLocation();
                map.IsVisible = true;
            }
        }

        CancellationTokenSource cts;
        public async void DisplayCurrentLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    cts = new CancellationTokenSource();
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest {
                        DesiredAccuracy = GeolocationAccuracy.Best,
                        Timeout = TimeSpan.FromSeconds(30)
                    }, cts.Token);
                }
                if (location != null)
                {
                    Position p = new Position(location.Latitude, location.Longitude);
                    MapSpan mapSpan = MapSpan.FromCenterAndRadius(p, Distance.FromMeters(444));
                    map.MoveToRegion(mapSpan);
                    map.MyLocationEnabled = true;

                    map.IsVisible = true;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            } 
        }
        protected override void OnDisappearing()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
            base.OnDisappearing();
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                if (width > height)
                {
                    outerStack.Orientation = StackOrientation.Horizontal;
                    outer2Stack.Margin = new Thickness(100, 0, 0, 0);
                }
                else
                {
                    outerStack.Orientation = StackOrientation.Vertical;
                    outer2Stack.Margin = new Thickness(0, 100, 0, 0);
                }
            }
        }
    }
}
