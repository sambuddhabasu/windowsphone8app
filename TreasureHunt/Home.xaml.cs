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
namespace TreasureHunt
{
    public partial class Home : PhoneApplicationPage
    {
        public Home()
        {
            InitializeComponent();
        }

        private void ProfileLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GameMenu.xaml", UriKind.Relative));
        }

        private void FriendsLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ComingSoon.xaml", UriKind.Relative));
        }

        private void JoinHunt_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["Pivot"] = "Join";
            NavigationService.Navigate(new Uri("/GameMenu.xaml", UriKind.Relative));
        }

        private void CreateHunt_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["Pivot"] = "Create";
            NavigationService.Navigate(new Uri("/GameMenu.xaml", UriKind.Relative));
        }
    }
}