using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Device.Location;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Devices;
using System.Text.RegularExpressions;

namespace TreasureHunt
{
    public partial class GamePage : PhoneApplicationPage
    {
        String Hint, Location;

        private MobileServiceCollection<hunts, hunts> game_hunt;
        private IMobileServiceTable<hunts> huntsTable = App.MobileService.GetTable<hunts>();
        public String GameID = "";
        public int Score = 0;
        public Geoposition geoposition;
        
        public GamePage()
        {
            InitializeComponent();
            getHintAndLocation();
            MyScore.Text = "0";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (PhoneApplicationService.Current.State.ContainsKey("GameID"))
            {
                GameID = (string)PhoneApplicationService.Current.State["GameID"];
            }
        }

        public class hunts
        {
            public String Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "creator")]
            public string Creator { get; set; }

            [JsonProperty(PropertyName = "location1")]
            public string Location1 { get; set; }

            [JsonProperty(PropertyName = "location2")]
            public string Location2 { get; set; }

            [JsonProperty(PropertyName = "location3")]
            public string Location3 { get; set; }

            [JsonProperty(PropertyName = "location4")]
            public string Location4 { get; set; }

            [JsonProperty(PropertyName = "location5")]
            public string Location5 { get; set; }

            [JsonProperty(PropertyName = "hint1")]
            public string Hint1 { get; set; }

            [JsonProperty(PropertyName = "hint2")]
            public string Hint2 { get; set; }

            [JsonProperty(PropertyName = "hint3")]
            public string Hint3 { get; set; }

            [JsonProperty(PropertyName = "hint4")]
            public string Hint4 { get; set; }

            [JsonProperty(PropertyName = "hint5")]
            public string Hint5 { get; set; }


        }

        private async void GetMapDetails()
        {

            game_hunt = await huntsTable
                .Where(hunts => hunts.Id == GameID)
                .ToCollectionAsync();

        }

        protected async void getHintAndLocation()
        {
            //magically gets the hint and location.
            GetMapDetails();
            MyHint.Text = game_hunt[0].Hint1;

        }

        protected async void refreshLocation()
        {
            if (Score == 0)
            {
                MyHint.Text = game_hunt[0].Hint1;
            }
            else if (Score == 1)
            {
                MyHint.Text = game_hunt[0].Hint2;
            }
            else if (Score == 2)
            {
                MyHint.Text = game_hunt[0].Hint3;
            }
            else if (Score == 3)
            {
                MyHint.Text = game_hunt[0].Hint4;
            }
            else if (Score == 4)
            {
                MyHint.Text = game_hunt[0].Hint5;
            }
            else
            {
                NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative));
            }
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                geoposition = await geolocator.GetGeopositionAsync(
                maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(60)
                );

                MyMap.Layers.Clear();
                MapLayer mapLayer = new MapLayer();

                // Draw marker for current position
                if (geoposition.Coordinate != null)
                {
                    //DrawAccuracyRadius(mapLayer);
                    DrawMapMarker(geoposition.Coordinate, Colors.Black, mapLayer);
                }
                Geocoordinate coordinate = geoposition.Coordinate;
                GeoCoordinate gc = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
                MyMap.Center = gc;
                MyMap.ZoomLevel = 15;
                MyMap.Layers.Add(mapLayer);
                //mapOverlayCanvas
            }
            catch (Exception e)
            {

            }
        }
        private void DrawMapMarker(Geocoordinate coordinate, Color color, MapLayer mapLayer)
        {
            // Create a map marker
            Polygon polygon = new Polygon();
            polygon.Points.Add(new Point(0, 0));
            polygon.Points.Add(new Point(0, 75));
            polygon.Points.Add(new Point(25, 0));
            polygon.Fill = new SolidColorBrush(color);

            // Enable marker to be tapped for location information
            polygon.Tag = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);

            //On click for the poygon.
            //polygon.MouseLeftButtonUp += new MouseButtonEventHandler(Marker_Click);

            // Create a MapOverlay and add marker
            MapOverlay overlay = new MapOverlay();
            overlay.Content = polygon;
            overlay.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            overlay.PositionOrigin = new Point(0.0, 1.0);
            mapLayer.Add(overlay);
        }
        protected void doCallBack()
        {
            checkProximity();
            System.Threading.ThreadPool.QueueUserWorkItem(obj =>
            {
                System.Threading.Thread.Sleep(60000); Dispatcher.BeginInvoke(() =>
              {
                  refreshLocation();
                  doCallBack();
              });
            });
        }

        public void checkProximity()
        {
        /*    Double distance;
            if (Score == 0)
            {
                string[] lines = Regex.Split(game_hunt[0].Location1, ",");
                distance = DistanceInMetres(double.Parse(lines[0]), double.Parse(lines[1]), geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if(distance <= 20) {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(3));
                    Score++;
                }
                if(distance <= 100) {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(1));
                }
            }
            else if (Score == 1)
            {
                string[] lines = Regex.Split(game_hunt[0].Location2, ",");
                distance = DistanceInMetres(double.Parse(lines[0]), double.Parse(lines[1]), geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (distance <= 20)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(3));
                    Score++;
                }
                if (distance <= 100)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(1));
                }
            }
            else if (Score == 2)
            {
                string[] lines = Regex.Split(game_hunt[0].Location3, ",");
                distance = DistanceInMetres(double.Parse(lines[0]), double.Parse(lines[1]), geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (distance <= 20)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(3));
                    Score++;
                }
                if (distance <= 100)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(1));
                }
            }
            else if (Score == 3)
            
                string[] lines = Regex.Split(game_hunt[0].Location4, ",");
                distance = DistanceInMetres(double.Parse(lines[0]), double.Parse(lines[1]), geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (distance <= 20)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(3));
                    Score++;
                }
                if (distance <= 100)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(1));
                }
            }
            else if (Score == 4)
            {
                string[] lines = Regex.Split(game_hunt[0].Location5, ",");
                distance = DistanceInMetres(double.Parse(lines[0]), double.Parse(lines[1]), geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (distance <= 20)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(3));
                    Score++;
                }
                if (distance <= 100)
                {
                    VibrateController vb = VibrateController.Default;
                    vb.Start(TimeSpan.FromSeconds(1));
                }
            }*/
        }
        public static Double DistanceInMetres(double lat1, double lon1, double lat2, double lon2)
        {

            if (lat1 == lat2 && lon1 == lon2)
                return 0.0;

            var theta = lon1 - lon2;

            var distance = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) +
                           Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                           Math.Cos(deg2rad(theta));

            distance = Math.Acos(distance);
            if (double.IsNaN(distance))
                return 0.0;

            distance = rad2deg(distance);
            distance = distance * 60.0 * 1.1515 * 1609.344;

            return (distance);
        }

        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
        
    }
}