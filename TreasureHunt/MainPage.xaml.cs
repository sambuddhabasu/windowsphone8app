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
using Windows.Devices.Geolocation;

namespace TreasureHunt
{
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
    public partial class MainPage : PhoneApplicationPage
    {

        private MobileServiceCollection<users, users> check_existing_email;
        private MobileServiceCollection<users, users> check_user_login;

        private IMobileServiceTable<users> usersTable = App.MobileService.GetTable<users>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void InsertNewUser(users newUser)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            signup_progress_bar.Visibility = Visibility.Visible;
            signup_first_name.IsEnabled = false;
            signup_last_name.IsEnabled = false;
            signup_email.IsEnabled = false;
            signup_password.IsEnabled = false;
            signup_verify_password.IsEnabled = false;
            signup_submit.IsEnabled = false;

            check_existing_email = await usersTable
                .Where(users => users.Email == signup_email.Text)
                .ToCollectionAsync();
            // If no previous accounts with the same email is present, new record has to be inserted
            if (check_existing_email.Count == 0)
            {
                await usersTable.InsertAsync(newUser);
                signup_progress_bar.Visibility = Visibility.Collapsed;
                signup_first_name.IsEnabled = true;
                signup_last_name.IsEnabled = true;
                signup_email.IsEnabled = true;
                signup_password.IsEnabled = true;
                signup_verify_password.IsEnabled = true;
                signup_submit.IsEnabled = true;
                signup_show_error.Text = "";
                MessageBox.Show("Registration successful.", "Congrats!", MessageBoxButton.OK);
                mainPivot.SelectedItem = LogIn;
            }
            else
            {
                signup_progress_bar.Visibility = Visibility.Collapsed;
                signup_first_name.IsEnabled = true;
                signup_last_name.IsEnabled = true;
                signup_email.IsEnabled = true;
                signup_password.IsEnabled = true;
                signup_verify_password.IsEnabled = true;
                signup_submit.IsEnabled = true;
                signup_show_error.Text = "Email already exists";
            }
        }

        private void signup_submit_Click(object sender, RoutedEventArgs e)
        {
            String error_message = "Invalid Input";
            if (signup_first_name.Text == "")
            {
                signup_first_name_error.Text = error_message;
            }
            else {
                signup_first_name_error.Text = "";
            }
            if (signup_last_name.Text == "")
            {
                signup_last_name_error.Text = error_message;
            }
            else {
                signup_last_name_error.Text = "";
            }
            if (signup_email.Text == "")
            {
                signup_email_error.Text = error_message;
            }
            else {
                signup_email_error.Text = "";
            }
            if (signup_password.Password == "")
            {
                signup_password_error.Text = error_message;
            }
            else {
                signup_password_error.Text = "";
            }
            if (signup_verify_password.Password == "")
            {
                signup_verify_password_error.Text = error_message;
            }
            else {
                signup_verify_password_error.Text = "";
            }
            if(signup_password.Password != signup_verify_password.Password) {
                signup_show_error.Text = "Passwords should match";
            }
            else {
                signup_show_error.Text = "";
            }
            if (signup_first_name.Text != "" && signup_last_name.Text != "" && signup_email.Text != "" && signup_password.Password != "" && signup_verify_password.Password != "" && signup_password.Password == signup_verify_password.Password)
            {

                var newUser = new users { FirstName = signup_first_name.Text,
                                          LastName = signup_last_name.Text,
                                          Email = signup_email.Text,
                                          Password = signup_password.Password,
                                          Hosted = 0.ToString(),
                                          Won = 0.ToString(),
                                          Played = 0.ToString()    };
                InsertNewUser(newUser);
                
            }
        }

        private void login_submit_Click(object sender, RoutedEventArgs e)
        {
            String error_message = "Invalid Input";
            if (login_email.Text == "")
            {
                login_email_error.Text = error_message;
            }
            else
            {
                login_email_error.Text = "";
            }
            if (login_password.Password == "")
            {
                login_password_error.Text = error_message;
            }
            else
            {
                login_password_error.Text = "";
            }
            if (login_email.Text != "" && login_password.Password != "")
            {
                UserLogin();
            }
        }

        private async void UserLogin()
        {
            login_show_error.Text = "";
            login_progress_bar.Visibility = Visibility.Visible;
            login_email.IsEnabled = false;
            login_password.IsEnabled = false;
            login_submit.IsEnabled = false;
            check_user_login = await usersTable
            .Where(users => users.Email == login_email.Text && users.Password == login_password.Password)
            .ToCollectionAsync();

            

            if (check_user_login.Count == 0)
            {
                login_show_error.Text = "Invalid email and password combination";
                login_email.IsEnabled = true;
                login_password.IsEnabled = true;
                login_submit.IsEnabled = true;
                login_progress_bar.Visibility = Visibility.Collapsed;
            }
            else
            {
                
                 Geoposition geoposition;
                 Geolocator geolocator = new Geolocator();
                geolocator.DesiredAccuracyInMeters = 50;
    
                foreach (users key in check_user_login)
                {
                    try
                    {
                        geoposition = await geolocator.GetGeopositionAsync(
                       maximumAge: TimeSpan.FromSeconds(5),
                       timeout: TimeSpan.FromSeconds(10)
                       );
                        key.Latitude = geoposition.Coordinate.Latitude.ToString();
                        key.Longitude = geoposition.Coordinate.Longitude.ToString();

                        await usersTable.UpdateAsync(key);
                        login_email.IsEnabled = true;
                        login_password.IsEnabled = true;
                        login_submit.IsEnabled = true;
                        login_progress_bar.Visibility = Visibility.Collapsed;
                        IsolatedStorageSettings.ApplicationSettings["Id"] = key.Id;
                        NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative));
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
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
    }
}