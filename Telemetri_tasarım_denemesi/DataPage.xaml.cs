using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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
using System.Windows.Threading;


namespace Telemetri_tasarım_denemesi
{
    public partial class DataPage : Page
    {
         DispatcherTimer timer;
        public DataPage()
        {
            InitializeComponent();
        }
        private void DataPage_Loaded(object sender, RoutedEventArgs e)
        {
        HizLbl.Content = $"{AppState.hiz} km/h";
        VoltajLbl.Content = $"{AppState.voltaj} V";
        DereceLbl.Content = $"{AppState.sicaklik} °C";
        whLbl.Content = $"{AppState.enerji} wh";
        PaketKaybi.Content = $"{AppState.KayipPaket} Tane Paket Kayıp";

            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += Timer_Tick; 
            }
        timer.Start();
        }
        private void DataPage_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            HizLbl.Content = $"{AppState.hiz} km/h";
            VoltajLbl.Content = $"{AppState.voltaj} V";
            DereceLbl.Content = $"{AppState.sicaklik} °C";
            whLbl.Content = $"{AppState.enerji} wh";
            PaketKaybi.Content = $"{AppState.KayipPaket} Tane Paket Kayıp";
            bool BayatVeri = (DateTime.Now - AppState.SonVeriZamani).TotalSeconds > 2;
            if (BayatVeri == true)
            {
                HizLbl.Content = "Bağlantı Koptu";
                VoltajLbl.Content = "Bağlantı Koptu";
                DereceLbl.Content = "Bağlantı Koptu";
                whLbl.Content = "Bağlantı Koptu";
            }
        }
    }
}