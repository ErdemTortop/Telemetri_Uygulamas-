using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Telemetri
{
    public partial class Gps_Page : Page
    {
        private bool followDot = true;
        private Map _map;
        private DispatcherTimer _gpsTimer;

        // Telemetri Başlangıç Değerleri (Ankara)
        private double _currentLon = 32.8541;
        private double _currentLat = 39.9208;
        private Random _random = new Random();

        public Gps_Page()
        {
            InitializeComponent();
            InitializeTelemetryMap();
            StartGpsSimulationTimer();
        }

        private void InitializeTelemetryMap()
        {
            _map = new Map();

            // Harita içi log panelini kapatır
            Mapsui.Logging.Logger.LogDelegate = null;

            // OpenStreetMap Taban Haritasını Ekle
            _map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MyMap.Map = _map;

            // KESİN ÇÖZÜM: Harita her hareket ettiğinde veya yakınlaştığında piksel konumunu güncelle
            MyMap.PropertyChanged += (sender, args) =>
            {
                // Yeni mimaride harita nesnesinin (Map) özellikleri değiştiğinde tetiklenir
                if (args.PropertyName == "Map")
                {
                    RefreshBlueDotPosition();
                }
            };

            // Haritanın dokunma/fare hareketlerini anlık yakalamak için Viewport değişikliğini izle
            _map.Navigator.ViewportChanged += (sender, args) =>
            {
                RefreshBlueDotPosition();
            };

            // İlk odaklanma ve yüksek yakınlaşma seviyesi (Street View)
            var (x, y) = SphericalMercator.FromLonLat(_currentLon, _currentLat);
            _map.Navigator.CenterOn(new MPoint(x, y));
            _map.Navigator.ZoomToLevel(15);
        }

        private void StartGpsSimulationTimer()
        {
            _gpsTimer = new DispatcherTimer();
            _gpsTimer.Interval = TimeSpan.FromSeconds(3);
            _gpsTimer.Tick += GpsTimer_Tick;
            _gpsTimer.Start();
        }

        private void GpsTimer_Tick(object sender, EventArgs e)
        {
            // Canlı GPS Hareket Simülasyonu GEÇİCİ
            _currentLon += (_random.NextDouble() - 0.5) * 0.001;
            _currentLat += (_random.NextDouble() - 0.5) * 0.001;

            //_currentLon += APPSTATE.X
            //_currentLat += APPSTATE.Y

            // Kamerayı yeni koordinata ortala
            if (followDot)
            {
                var (x, y) = SphericalMercator.FromLonLat(_currentLon, _currentLat);
                _map.Navigator.CenterOn(new MPoint(x, y));
            }

            // Noktanın piksel konumunu güncelle
            RefreshBlueDotPosition();
        }

        private void RefreshBlueDotPosition()
        {
            // Güvenlik Kontrolü: Nesneler henüz yüklenmediyse işlemi pas geç
            if (MyMap?.Map?.Navigator?.Viewport == null) return;

            // KESİN ÇÖZÜM: İşlemi WPF ana arayüz iş parçacığına (UI Thread) gönder
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 1. Mevcut GPS koordinatını haritanın dünya metre birimine çevir
                    var (worldX, worldY) = SphericalMercator.FromLonLat(_currentLon, _currentLat);

                    // 2. Dünya koordinatını ekrandaki piksele çevir
                    var screenPoint = MyMap.Map.Navigator.Viewport.WorldToScreen(worldX, worldY);

                    // 3. Merkez ofsetini düş (16px / 2 = 8)
                    double pixelX = screenPoint.X - 8;
                    double pixelY = screenPoint.Y - 8;

                    // 4. WPF Canvas üzerinde nesneyi güvenli bir şekilde konumlandır
                    Canvas.SetLeft(WpfBlueDot, pixelX);
                    Canvas.SetTop(WpfBlueDot, pixelY);
                }
                catch (Exception)
                {
                    // Sayfa kapanırken veya nesneler yok edilirken oluşabilecek istisnaları yakalar
                }
            }));
        }

        private void GpsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void GpshPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_gpsTimer != null)
            {
                _gpsTimer.Stop();
                _gpsTimer.Tick -= GpsTimer_Tick;
            }
        }

        private void takip_et_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            followDot = !followDot;
            takip_et.Content = followDot;
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
