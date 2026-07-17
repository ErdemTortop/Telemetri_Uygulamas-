using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telemetri;

namespace Telemetri_tasarım_denemesi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new HomePage());
            SolarBorder.Background = Brushes.Transparent;
            UsbBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;
            dataBorder.Background = Brushes.Transparent;
            RecordBorder.Background = Brushes.Transparent;
            HomeBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));

        }
      
        private void Usb_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsbPage());
            SolarBorder.Background = Brushes.Transparent;
            UsbBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));
            dataBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
            RecordBorder.Background = Brushes.Transparent;
            HomeBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;

        }

        private void Data_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DataPage());
            SolarBorder.Background = Brushes.Transparent;
            HomeBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;
            UsbBorder.Background = Brushes.Transparent;
            dataBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));
            RecordBorder.Background = Brushes.Transparent;
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CameraPage());
            SolarBorder.Background = Brushes.Transparent;
            UsbBorder.Background = Brushes.Transparent;
            HomeBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;
            dataBorder.Background = Brushes.Transparent;
            RecordBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));
        }

        private void SolarTeam_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TeamPage());
            SolarBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));
            UsbBorder.Background = Brushes.Transparent;
            dataBorder.Background = Brushes.Transparent;
            RecordBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;
            HomeBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
        }

        public async Task<bool> IsInternetAvailableAsync()
        {
            using (var client = new HttpClient())
            {
                // Set a short timeout so your UI doesn't freeze waiting for a dead connection
                client.Timeout = TimeSpan.FromSeconds(3);

                try
                {
                    // Request just the headers from a highly reliable server to save data
                    using (var response = await client.GetAsync("https://google.com", HttpCompletionOption.ResponseHeadersRead))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
                catch
                {
                    // Any network exception means the internet is unreachable
                    return false;
                }
            }
        }

        private async void Home_Click(object sender, RoutedEventArgs e)
        {
            SolarBorder.Background = Brushes.Transparent;
            UsbBorder.Background = Brushes.Transparent;
            dataBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;
            RecordBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
            HomeBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));

            bool hasInternet = await IsInternetAvailableAsync();

            if (hasInternet)
            {
                // Proceed with loading your WebView2
                MainFrame.Navigate(new HomePage());
            }
            else
            {
                // Handle offline mode (e.g., show a local HTML file or an error message)
                System.Windows.MessageBox.Show("No internet connection detected. Please check your network.");
                MainFrame.Navigate(new HomePageNoInt());
            }
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Graph_Page());
            SolarBorder.Background = Brushes.Transparent;
            UsbBorder.Background = Brushes.Transparent;
            dataBorder.Background = Brushes.Transparent;
            GpsBorder.Background = Brushes.Transparent;
            RecordBorder.Background = Brushes.Transparent;
            HomeBorder.Background = Brushes.Transparent;
            GraphBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));
        }

        private void Gps_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Gps_Page());
            SolarBorder.Background = Brushes.Transparent;
            GpsBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 31));
            UsbBorder.Background = Brushes.Transparent;
            dataBorder.Background = Brushes.Transparent;
            RecordBorder.Background = Brushes.Transparent;
            HomeBorder.Background = Brushes.Transparent;
            GraphBorder.Background = Brushes.Transparent;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.RecordFlag == true)
            {
                //MessageBox.Show("Hâlâ kayıt yapılmaktadır", "Uyarı !", MessageBoxButton.OK);
                if (MessageBox.Show("Hâlâ kayıt yapılmaktadır.\nKapatmak istediğinize emin misiniz?", "Uyarı!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        
    }
}