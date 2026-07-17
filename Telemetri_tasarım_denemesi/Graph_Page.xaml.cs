using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Telemetri_tasarım_denemesi;

namespace Telemetri
{
    /// <summary>
    /// Interaction logic for Graph_Page.xaml
    /// </summary>
    public partial class Graph_Page : Page
    {
        // Define observable tracking lists globally so the timer can access them
        private ObservableCollection<double> _speedValues = new ObservableCollection<double>();
        private ObservableCollection<double> _tempValues = new ObservableCollection<double>();
        private ObservableCollection<double> _voltValues = new ObservableCollection<double>();

        // Define the live refresh timer
        private DispatcherTimer _realtimeTimer;

        string logPath = AppState.KayıtDosya;

        public Graph_Page()
        {
            InitializeComponent();
        }

        private void GraphPage_Loaded(object sender, RoutedEventArgs e)
        {
            // --- STEP 3: Initial Data Load (Last 60 Points) ---
            bool successfullyLoaded = false;

            if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
            {
                try
                {
                    string[] allLines;
                    using (var fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        var list = new List<string>();
                        while (!streamReader.EndOfStream)
                        {
                            list.Add(streamReader.ReadLine());
                        }
                        allLines = list.ToArray();
                    }

                    var targetLines = allLines
                        .Where(line => !line.Contains("Zaman_ms"))
                        .Skip(Math.Max(0, allLines.Length - 61))
                        .Take(60);

                    foreach (var line in targetLines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] tokens = line.Split(';');
                        if (tokens.Length >= 4)
                        {
                            string speedRaw = tokens[1].Replace(" km/h", "").Trim();
                            if (double.TryParse(speedRaw, System.Globalization.CultureInfo.InvariantCulture, out double speed))
                                _speedValues.Add(speed);

                            string tempRaw = tokens[2].Replace(" °C", "").Trim();
                            if (double.TryParse(tempRaw, System.Globalization.CultureInfo.InvariantCulture, out double temp))
                                _tempValues.Add(temp);

                            string voltRaw = tokens[3].Replace(" V", "").Trim();
                            if (double.TryParse(voltRaw, System.Globalization.CultureInfo.InvariantCulture, out double volt))
                                _voltValues.Add(volt);
                        }
                    }
                    // Confirm we successfully read actual historical data points
                    if (_speedValues.Count > 0)
                    {
                        successfullyLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Graph Load Error] {ex.Message}");
                    // Exception caught; successfullyLoaded remains false to trigger fallback
                }
            }

            // Assign collections to charts
            Chart1.Series = new ISeries[] { new LineSeries<double> { Name = "Hız (km/h)", Values = _speedValues, GeometrySize = 0, Fill = null } };
            Chart2.Series = new ISeries[] { new LineSeries<double> { Name = "Sıcaklık (°C)", Values = _tempValues, GeometrySize = 0, Fill = null } };
            Chart3.Series = new ISeries[] { new LineSeries<double> { Name = "Voltaj (V)", Values = _voltValues, GeometrySize = 0, Fill = null } };

            // --- STEP 4: Start UI Update Dispatcher Timer ---
            _realtimeTimer = new DispatcherTimer();
            _realtimeTimer.Interval = TimeSpan.FromSeconds(1); // Triggers exactly once every second
            _realtimeTimer.Tick += RealtimeTimer_Tick;
            _realtimeTimer.Start();
        }

        private void RealtimeTimer_Tick(object sender, EventArgs e)
        {
            // 1. Pull the newest single values arriving live into AppState variables
            double liveSpeed = AppState.hiz;
            double liveTemp = AppState.sicaklik;
            double liveVolt = AppState.voltaj;

            // 2. Append the new values to the active collections
            _speedValues.Add(liveSpeed);
            _tempValues.Add(liveTemp);
            _voltValues.Add(liveVolt);

            // 3. Keep a rolling window: if data exceeds 60 points, drop the oldest point
            if (_speedValues.Count > 60) _speedValues.RemoveAt(0);
            if (_tempValues.Count > 60) _tempValues.RemoveAt(0);
            if (_voltValues.Count > 60) _voltValues.RemoveAt(0);
        }

        private void GraphPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // --- STEP 5: Safe Cleanup ---
            // Stop the timer when leaving the page to prevent memory leaks and background CPU cycles
            if (_realtimeTimer != null)
            {
                _realtimeTimer.Stop();
                _realtimeTimer.Tick -= RealtimeTimer_Tick;
            }
        }


        public class MainViewModel
        {
            // 1. Define the Series collection for the Chart
            public ISeries[] MySeries { get; set; }
            // 2. Define Custom X-Axis Labels
            public Axis[] XAxes { get; set; }

            public ISeries[] MySeries2 { get; set; }
            public Axis[] XAxes2 { get; set; }

            public ISeries[] MySeries3 { get; set; }
            public Axis[] XAxes3 { get; set; }

            public MainViewModel()
            {
                // Populate data points
                MySeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Name = "Speed Tracker",
                        Values = new ObservableCollection<double> { 1200, 1500, 1100, 1900 },
                        Fill = null // Removes area shading under the line
                    }
                };

                // Map string labels to the grid
                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = new string[] { "Jan", "Feb", "Mar", "Apr" }
                    }
                };

                // Populate data points
                MySeries2 = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Name = "Speed Tracker",
                        Values = new ObservableCollection<double> { 1200, 1500, 1100, 1900 },
                        Fill = null // Removes area shading under the line
                    }
                };

                // Map string labels to the grid
                XAxes2 = new Axis[]
                {
                    new Axis
                    {
                        Labels = new string[] { "Jan", "Feb", "Mar", "Apr" }
                    }
                };

                // Populate data points
                MySeries3 = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Name = "Speed Tracker",
                        Values = new ObservableCollection<double> { 1200, 1500, 1100, 1900 },
                        Fill = null // Removes area shading under the line
                    }
                };

                // Map string labels to the grid
                XAxes3 = new Axis[]
                {
                    new Axis
                    {
                        Labels = new string[] { "Jan", "Feb", "Mar", "Apr" }
                    }
                };
            }
        }

    }
}
