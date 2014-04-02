using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TreasureHunt
{
    public partial class ComingSoon : PhoneApplicationPage
    {
        public ComingSoon()
        {
            InitializeComponent();
        }

        private void HomeLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}