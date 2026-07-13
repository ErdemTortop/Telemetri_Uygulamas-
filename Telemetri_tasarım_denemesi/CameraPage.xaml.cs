using System;
using System.Collections.Generic;
using System.IO;
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
using System.IO.Ports;

namespace Telemetri_tasarım_denemesi
{
    /// <summary>
    /// Interaction logic for CameraPage.xaml
    /// </summary>
    public partial class CameraPage : Page
    {

        DispatcherTimer timer;


        
        public CameraPage()
        {

            InitializeComponent();

        }


        private void CameraPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += Timer_Tick; 
            }
            timer.Start();
            if (AppState.RecordFlag == true)
            {
                KayıtDurumuRenk.Background = (Brush)new BrushConverter().ConvertFrom("#4CAF7D");
            }
        }


        private void CameraPage_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void Kayit_Click(object sender, RoutedEventArgs e)
        {

            if (AppState.RecordFlag == false)
            {

                if (AppState.SerialPort != null && AppState.SerialPort.IsOpen)
                {
                    string belgelerim = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    AppState.dosyaYolu = System.IO.Path.Combine(belgelerim, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
                    AppState.ExKayıtDosya = AppState.dosyaYolu;
                    AppState.RecordFlag = true;
                    KayıtDurumuRenk.Background = (Brush)new BrushConverter().ConvertFrom("#4CAF7D");
                }
                else
                {
                    MessageBox.Show("Selamlar, eğer daha önce hata almadıysan ben Erdem. Tahminimce seri port bağlı değilken verileri kaydetmek istedin ama 0, 0, 0, 0 kaydediceksin. Bence önce port bağlandısını USB sekmesinden tamamla.", "Error");
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.RecordFlag == true)
            {
                if (!AppState.yazimKuyrugu.IsEmpty)
                {
                    StringBuilder sb = new StringBuilder();
                    while (AppState.yazimKuyrugu.TryDequeue(out var satir))
                    {
                        sb.AppendLine(satir);
                    }
                    File.AppendAllText(AppState.KayıtDosya, sb.ToString(), System.Text.Encoding.UTF8);
                }

                AppState.RecordFlag = false;
                AppState.BaslıkYazıldi = false;
                AppState.dosyaYolu = "";
                AppState.KayıtDosya = "";
                KayıtDurumuRenk.Background = (Brush)new BrushConverter().ConvertFrom("#E05C5C");
            }
        }


        private async void FileTemizle_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                if (AppState.RecordFlag == false)
                {
                    if (File.Exists(AppState.ExKayıtDosya))
                    {
                        File.Delete(AppState.ExKayıtDosya);
                        KayitTextBox.Text = "";
                        KayitTextBox.Text = "Kayıtlar Temizlendi";
                        await Task.Delay(1500);
                        KayitTextBox.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AppState.RecordFlag == true)
            {
                    KayitTextBox.Text += $"{DateTime.Now:HH:mm:ss}" +
                            $", Hız: {AppState.hiz} km/h" +
                            $", Voltaj: {AppState.voltaj} V" +
                            $", Sıcaklık: {AppState.sicaklik} °C" +
                            $", Enerji: {AppState.enerji} Wh\n" + 
                            $"{AppState.ExKayıtDosya}\n";
            }
        }


        private void File_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe",
                System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(AppState.ExKayıtDosya)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selamlar, Ben Erdem. Şimdi muhtemelen programın çökmesine sebep olan bir hata ile karşılaştın. En kısa sürede getiriceğimiz bir güncelleme ile hataları ayıklicaz. Lütfen programı baştan çalıştırır mısınız? Yada muhtemelen arkada açık olan excel dosyasını kapatırsanız sorun çözülücektir :)", ex.Message);
            }
        }
    }
}





