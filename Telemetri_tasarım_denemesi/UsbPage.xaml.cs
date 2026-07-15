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
using System.IO.Ports;

namespace Telemetri_tasarım_denemesi
{
    /// <summary>
    /// Interaction logic for UsbPage.xaml
    /// </summary>

    public partial class UsbPage : Page
    {

        public UsbPage()
        {
            InitializeComponent();
        }


        private async Task UyarıBlink(string fisrtColor, string secondColor, string lastColor)
        {
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(fisrtColor);
            await Task.Delay(300);
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(secondColor);
            await Task.Delay(300);
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(fisrtColor);
            await Task.Delay(300);
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(secondColor);
            await Task.Delay(300);
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(fisrtColor);
            await Task.Delay(300);
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(secondColor);
            await Task.Delay(300);
            BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom(lastColor);
        }

        private void UsbPage_Loaded(object sender, RoutedEventArgs e)
        {
            AppState.DurumDegisti += Durum_Degisti;
            foreach (string port in SerialPort.GetPortNames())
            {
                PortComboBox.Items.Add(port);
            }
            RateBox.SelectedItem = "9600";
            RateBox.Items.Add("9600");
            RateBox.Items.Add("115200");
            RateBox.Items.Add("19200");
            RateBox.Items.Add("57600");
            

            if (AppState.SerialPort != null && AppState.SerialPort.IsOpen)
            {
                BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom("#4CAF7D");
            }

            if (AppState.SecilenPort != null)
            {
                PortComboBox.SelectedItem = AppState.SecilenPort;
            }
            
            if (AppState.SecilenRate != null)
            {
                RateBox.SelectedItem = AppState.SecilenRate;
            }
        }
        private async void PortBaglanButon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PortComboBox.SelectedItem != null && RateBox.SelectedItem != null)
                {
                    if (AppState.SerialPort == null || !AppState.SerialPort.IsOpen)
                    {
                        string portDeger = PortComboBox.SelectedItem.ToString();
                        int rateDeger = int.Parse(RateBox.SelectedItem.ToString());
                        AppState.SecilenPort = portDeger;
                        AppState.SecilenRate = rateDeger.ToString();
                        AppState.SerialPort = new SerialPort(portDeger, rateDeger);
                        AppState.SerialPort.Open();
                        AppState.StartListening();
                        BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom("#4CAF7D");
                    }
                    else
                    {
                        MessageBox.Show("Dostum, seçtiğin port doluymuş zaten.");
                        await UyarıBlink("#E6B84A", "#262835", "#4CAF7D");
                    }                
                }
                else
                {
                    MessageBox.Show("Dostum, ya baud rate seçmedin yada port seçmedin.", "Error  ");
                    await UyarıBlink("#E6B84A", "#262835", "#E05C5C" );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Ufak Bir hata oluştu");
                await UyarıBlink("#E6B84A", "#262835", "#E05C5C");
            }
        }
        private async void PortIptalButon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppState.SerialPort != null && AppState.SerialPort.IsOpen)
                {
                    AppState.StopListening();
                    AppState.SerialPort.Close();
                    BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom("#E05C5C");
                }
                else
                {
                    MessageBox.Show("Dostum, daha port seçmedin ki");
                    await UyarıBlink("#E6B84A", "#262835", "#E05C5C");
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "Ufak Bir hata oluştu");
                await UyarıBlink("#E6B84A", "#262835", "#E05C5C");
            }
        }

        private void PortYenileButon_Click(object sender, RoutedEventArgs e)
        {
            PortComboBox.Items.Clear();
            RateBox.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                PortComboBox.Items.Add(port);
            }
            RateBox.Items.Add("9600");
            RateBox.Items.Add("115200");
            RateBox.Items.Add("19200");
            RateBox.Items.Add("57600");
        }
        private void Durum_Degisti(AppState.BaglantiDurumu yeniDurum)
        {
            Dispatcher.Invoke(() =>
            {
                switch (yeniDurum)
                {
                    case AppState.BaglantiDurumu.Bagli:
                        BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom("#4CAF7D");
                        break;
                    case AppState.BaglantiDurumu.Kopuk:
                        BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom("#E05C5C");
                        break;
                    case AppState.BaglantiDurumu.YenidenBaglaniyor:
                        BaglantıDurumuLblRenk.Background = (Brush)new BrushConverter().ConvertFrom("#E6B84A");
                        break;
                }
            });
        }

        private void UsbPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AppState.DurumDegisti -= Durum_Degisti;
        }
    }
}




