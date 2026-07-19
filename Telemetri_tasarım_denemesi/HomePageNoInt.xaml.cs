using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Telemetri_tasarım_denemesi
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePageNoInt : Page
    {
        public HomePageNoInt()
        {
            InitializeComponent();  
        }

        private void HomePageNoIntLoaded(object sender, RoutedEventArgs e)
        {
            VersionLabel.Text = Telemetri.Properties.Settings.Default.TelemetriVersion;
        }
    }
}
