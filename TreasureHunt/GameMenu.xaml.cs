using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TreasureHunt.Resources;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

using System.IO.IsolatedStorage;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;
using Microsoft.Devices;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Shapes;

namespace TreasureHunt
{
    public partial class GameMenu : PhoneApplicationPage
    {
        public int count = 0;
        public String GameIndex = "";
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
        public class users
        {
            public String Id { get; set; }

            [JsonProperty(PropertyName = "first_name")]
            public string FirstName { get; set; }

            [JsonProperty(PropertyName = "last_name")]
            public string LastName { get; set; }

            [JsonProperty(PropertyName = "email")]
            public string Email { get; set; }

            [JsonProperty(PropertyName = "password")]
            public string Password { get; set; }

            [JsonProperty(PropertyName = "latitude")]
            public string Latitude { get; set; }

            [JsonProperty(PropertyName = "longitude")]
            public string Longitude { get; set; }

            [JsonProperty(PropertyName = "hosted")]
            public string Hosted { get; set; }

            [JsonProperty(PropertyName = "played")]
            public string Played { get; set; }

            [JsonProperty(PropertyName = "won")]
            public string Won { get; set; }

        }
        MapLayer mapLayer;
        Geoposition geoposition;
        public void LetsPlay(object sender, SelectionChangedEventArgs e)
        {

            PhoneApplicationService.Current.State["GameID"] = HuntsList.SelectedItem.ToString();
            NavigationService.Navigate(new Uri("/GamePage.xmal", UriKind.Relative));
        }

        private MobileServiceCollection<users, users> get_name;
        private IMobileServiceTable<users> usersTable = App.MobileService.GetTable<users>();

        private MobileServiceCollection<hunts, hunts> get_hunt;
        private MobileServiceCollection<hunts, hunts> show_hunts;
        private IMobileServiceTable<hunts> huntsTable = App.MobileService.GetTable<hunts>();

        public String Hint1 = "";
        public String Hint2 = "";
        public String Hint3 = "";
        public String Hint4 = "";
        public String Hint5 = "";
        public String Location1 = "";
        public String Location2 = "";
        public String Location3 = "";
        public String Location4 = "";
        public String Location5 = "";

        // Constructor
        public GameMenu()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            GetDetails();
            StatsLink.Visibility = Visibility.Visible;
            LoadCurrentLocation();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (PhoneApplicationService.Current.State.ContainsKey("Pivot"))
            {
                if ((string)PhoneApplicationService.Current.State["Pivot"] == "Join")
                {
                    mainPivot.SelectedItem = JoinPivot;
                }
                else if ((string)PhoneApplicationService.Current.State["Pivot"] == "Create")
                {
                    mainPivot.SelectedItem = CreatePivot;
                }
            }

          //  VibrateController vb = VibrateController.Default;
          //   vb.Start(TimeSpan.FromSeconds(3));
            
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }   
            LoadCurrentLocation();
        }

        


        private async void LoadCurrentLocation()
        {

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                geoposition = await geolocator.GetGeopositionAsync(
                maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
                );

             //   LatitudeTextBlock.Text = geoposition.Coordinate.Latitude.ToString("0.00");
             //   LongitudeTextBlock.Text = geoposition.Coordinate.Longitude.ToString("0.00");
                MyMap.Layers.Clear();
                mapLayer = new MapLayer();

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
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                 //   StatusTextBlock.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
         //   LatitudeTextBlock.Text = MyMap.Center.Latitude.ToString();
         //   LongitudeTextBlock.Text = MyMap.Center.Longitude.ToString();
         //   GeoCoordinate gc = new GeoCoordinate(MyMap.Center.Latitude, MyMap.Center.Longitude);
         //   markPoint(gc);
        }

        private void markPoint(GeoCoordinate gc)
        {
            Ellipse ell = new Ellipse();
            ell.Width = 15;
            ell.Height = 15;
            ell.Fill = new SolidColorBrush(Colors.Red);
            MapOverlay overlay = new MapOverlay();
            overlay.Content = ell;
            overlay.GeoCoordinate = new GeoCoordinate(MyMap.Center.Latitude, MyMap.Center.Longitude);
            overlay.PositionOrigin = new Point(0.0, 0.0);
            mapLayer.Add(overlay);
        }
        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}


        private async void GetDetails()
        {
            get_name = await usersTable
            .Where(users => users.Id == IsolatedStorageSettings.ApplicationSettings["Id"])
            .ToCollectionAsync();

            foreach (users key in get_name)
            {
                profile_first_name.Text = key.FirstName;
                profile_last_name.Text = key.LastName;
                hunts_created.Text = key.Hosted;
                hunts_played.Text = key.Played;
                hunts_won.Text = key.Won;
            }
        }

        private void FriendsLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ComingSoon.xaml", UriKind.Relative));
        }

        private void FBLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ComingSoon.xaml", UriKind.Relative));
        }

        private void StatsLink_Click(object sender, RoutedEventArgs e)
        {
            StatsLink.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeoCoordinate gc = new GeoCoordinate(MyMap.Center.Latitude, MyMap.Center.Longitude);
            count++;
            point_canvas.Visibility = Visibility.Visible;
            point_clue.Text = "";
            markPoint(gc);
        }


        private async void InsertNewHunt(hunts newHunt)
        {
            await huntsTable.InsertAsync(newHunt);
            NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative));
        }

        private void point_submit_Click(object sender, RoutedEventArgs e)
        {
            if (count == 1)
            {
                Location1 = geoposition.Coordinate.Latitude.ToString() + ',' + geoposition.Coordinate.Longitude.ToString();
                Hint1 = point_clue.Text;
            }
            else if (count == 2)
            {
                Location2 = geoposition.Coordinate.Latitude.ToString() + ','+ geoposition.Coordinate.Longitude.ToString();
                Hint2 = point_clue.Text;
            }
            else if (count == 3)
            {
                Location3 = geoposition.Coordinate.Latitude.ToString() + geoposition.Coordinate.Longitude.ToString();
                Hint3 = point_clue.Text;
            }
            else if (count == 4)
            {
                Location4 = geoposition.Coordinate.Latitude.ToString() + geoposition.Coordinate.Longitude.ToString();
                Hint4 = point_clue.Text;
            }
            if (count == 5)
            {
                Location5 = geoposition.Coordinate.Latitude.ToString() + geoposition.Coordinate.Longitude.ToString();
                Hint5 = point_clue.Text;
                var hunt_data = new hunts { 
                Hint1 = Hint1,
                Hint2 = Hint2,
                Hint3 = Hint3,
                Hint4 = Hint4,
                Hint5 = Hint5,
                Location1 = Location1,
                Location2 = Location2,
                Location3 = Location3,
                Location4 = Location4,
                Location5 = Location5,
                Creator = IsolatedStorageSettings.ApplicationSettings["Id"].ToString()
                };
                InsertNewHunt(hunt_data);
            }
            point_canvas.Visibility = Visibility.Collapsed;
        }
        protected void doCallBack()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(obj =>
                {
                    System.Threading.Thread.Sleep(5000);

                    Dispatcher.BeginInvoke(() =>
                        {

                            doCallBack();
                        });
                });

        }
        private async void LoadHunts()
        {
            get_hunt = await huntsTable
                .ToCollectionAsync();
            List<Button> just_checking = new List<Button> { };
            foreach (hunts key in get_hunt)
            {
                Button checking = new Button();
                checking.Content = key.Id;
                
            //    if (checking.IsPressed)
              //      checking.Content = "ookoko";
               
                just_checking.Add(checking);
            }
             HuntsList.ItemsSource = just_checking;
          //  HuntsList.SelectionChanged = lets_play(HuntsList.SelectedItem.ToString());
        }
        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            LoadHunts();
           // ls = new List<String> { "One", "Two", "Three" };

          //  testList.ItemsSource = ls;
        }
        public async void getHunts()
        {
            get_hunt = await huntsTable
                .ToCollectionAsync();
        }
        private void PlaySubmit_Click(object sender, RoutedEventArgs e)
        {
            GameIndex = get_hunt[int.Parse(GameID.Text) - 1].Id;
            PhoneApplicationService.Current.State["GameID"] = GameIndex;
            NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }

        // Load data for the ViewModel Items
      

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}