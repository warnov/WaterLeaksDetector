using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FlowMonitorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        DispatcherTimer _timer;
        double _source;
        double _dest;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
            cityMap.Style = MapStyle.Aerial3D;
            BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 47.604, Longitude = -122.329 };
            Geopoint cityCenter = new Geopoint(cityPosition);

            cityMap.Center = cityCenter;
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    //_rootPage.NotifyUser("Waiting for update...", NotifyType.StatusMessage);

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    geolocator.StatusChanged += Geolocator_StatusChanged; 

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    //cityMap.Center = pos.Coordinate.Point;
                    var geoPoint =
                        pos.Coordinate.Point;
                        //new Geopoint(new BasicGeoposition() { Latitude = 26.208952, Longitude = -80.140717 });

                    cityMap.Center = geoPoint;

                    //26.208952, -80.140717
                    //UpdateLocationData(pos);
                    //_rootPage.NotifyUser("Location updated.", NotifyType.StatusMessage);

                    //BasicGeoposition snPosition = new BasicGeoposition() { Latitude = 47.620, Longitude = -122.349 };
                    //Geopoint snPoint = new Geopoint(snPosition);

                    // Create a MapIcon.
                    MapIcon mapIcon1 = new MapIcon();
                    mapIcon1.Location = geoPoint;
                    mapIcon1.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    mapIcon1.Title = "Principal Line";
                    mapIcon1.ZIndex = 0;
                    //var img=RandomAccessStreamReference.CreateFromUri(new Uri(
                    //    "https://d30y9cdsu7xlg0.cloudfront.net/png/53646-200.png"));
                    ////"https://cdn1.iconfinder.com/data/icons/Map-Markers-Icons-Demo-PNG/256/Map-Marker-Marker-Inside-Azure.png"));
                    
                    //mapIcon1.Image = img;
                    

                    cityMap.MapElements.Add(mapIcon1);
                    //26.200642, -80.134911
                    MapIcon mapIcon2 = new MapIcon();

                    mapIcon2.Location = new Geopoint(new BasicGeoposition() { Latitude = 26.200642, Longitude = -80.134911 });
                    mapIcon2.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    mapIcon2.Title = "Water provider";
                    mapIcon2.ZIndex = 0;
                    // Add the MapIcon to the map.
                    cityMap.MapElements.Add(mapIcon2);
                    var mapScene = MapScene.CreateFromLocationAndRadius(cityMap.Center, 200, 135, 65);
                    await cityMap.TrySetSceneAsync(mapScene);

                   
                    // Center the map over the POI.
                    

                    break;

                case GeolocationAccessStatus.Denied:
                    //_rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                    //LocationDisabledMessage.Visibility = Visibility.Visible;
                    //UpdateLocationData(null);
                    break;

                case GeolocationAccessStatus.Unspecified:
                    //_rootPage.NotifyUser("Unspecified error.", NotifyType.ErrorMessage);
                    //UpdateLocationData(null);
                    break;
            }
        }

        private void _timer_Tick(object sender, object e)
        {
            HttpClient client = new HttpClient();
            var json=client.GetStringAsync("http://localhost:16441/api/CityAlarm").Result;
            var response=JsonConvert.DeserializeObject<IEnumerable<double>>(json).ToArray<double>();
            _source = response[0];
            _dest = response[1];
            //_source++;
            //_dest++;
            //if (_source == 10)
            //    _source++;
            updateCounter();
        }

        private void updateCounter()
        {
            txtSource.Text = $"{_source} lt";
            txtDest.Text = $"{_dest} lt";
            if (_source != _dest)
                alarm.Visibility = Visibility.Visible;
            else
                alarm.Visibility = Visibility.Collapsed;
        }

        private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}
