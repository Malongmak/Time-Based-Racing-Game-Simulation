using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CSharpSpeedRush.Models;
using CSharpSpeedRush.Services;
using System.Linq; // Added for .Select()

namespace CSharpSpeedRush
{
    public partial class MainWindow : Window
    {
        private RaceManager raceManager;
        private DispatcherTimer raceTimer;
        private bool gameActive = false;
        private List<Car> aiCars;
        private readonly int trackLength = 700; // pixels
        private readonly int carHeight = 40;
        private readonly int carSpacing = 20;
        private readonly string[] carImageFiles = new string[] { "Assets/car_red.png", "Assets/car_blue.png", "Assets/car_green.png", "Assets/car_gold.png" };
        private readonly Dictionary<Car, string> carImageMap = new();
        private MediaPlayer winPlayer = new MediaPlayer();
        private int tickCount = 0;

        public MainWindow()
        {
            try
        {
            InitializeComponent();
            CarSelector.Items.Add(new Car("Speedster", 100, 40, 10));
            CarSelector.Items.Add(new Car("EcoCar", 80, 25, 5));
            CarSelector.Items.Add(new Car("Balanced", 90, 30, 7));
                this.Loaded += (s, e) =>
                {
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                    this.Topmost = true;
                    this.Topmost = false;
                    this.Focus();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup error: {ex.Message}");
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            if (CarSelector.SelectedItem is Car selectedCar)
            {
                // Create AI cars
                aiCars = new List<Car>
                {
                    new Car("AI Red", 100, 38, 10),
                    new Car("AI Blue", 90, 32, 8),
                    new Car("AI Green", 95, 35, 9)
                };
                raceManager = new RaceManager(selectedCar, aiCars);
                gameActive = true;
                StatusText.Text = "Race Started!";
                AssignCarImages(selectedCar, aiCars);
                StartRaceAnimation();
            }
            else
            {
                MessageBox.Show("Please select a car.");
            }
        }

        private void AssignCarImages(Car playerCar, List<Car> aiCars)
        {
            carImageMap.Clear();
            carImageMap[playerCar] = carImageFiles[3]; // gold for player
            carImageMap[aiCars[0]] = carImageFiles[0]; // red
            carImageMap[aiCars[1]] = carImageFiles[1]; // blue
            carImageMap[aiCars[2]] = carImageFiles[2]; // green
        }

        private void StartRaceAnimation()
        {
            if (raceTimer != null)
                raceTimer.Stop();
            raceTimer = new DispatcherTimer();
            raceTimer.Interval = TimeSpan.FromMilliseconds(100);
            raceTimer.Tick += RaceTimer_Tick;
            tickCount = 0;
            raceTimer.Start();
        }

        private void RaceTimer_Tick(object sender, EventArgs e)
        {
            tickCount++;
            this.Title = $"C# Speed Rush - Tick: {tickCount}";
            // Player AI: always Maintain (or you can add UI for player action)
            raceManager.AdvanceRace(RaceAction.Maintain);
            DrawRace();
            if (raceManager.RaceFinished)
            {
                raceTimer.Stop();
                ShowWinner();
            }
        }

        private void DrawRace()
        {
            try
            {
                RaceCanvas.Children.Clear();
                int i = 0;
                double finishX = trackLength + 30;
                // Draw finish line
                var finishLine = new Rectangle
                {
                    Width = 8,
                    Height = RaceCanvas.Height,
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                RaceCanvas.Children.Add(finishLine);
                Canvas.SetLeft(finishLine, finishX);
                Canvas.SetTop(finishLine, 0);

                // Prepare leaderboard
                var leaderboard = new List<(Car car, double progress, int lap, double lapProgress)>();

                foreach (var car in raceManager.Cars)
                {
                    var track = raceManager.CarTracks[car];
                    double progress = ((track.CurrentLap - 1) * 100 + track.LapProgress) / (track.TotalLaps * 100.0);
                    leaderboard.Add((car, progress, track.CurrentLap, track.LapProgress));
                }
                leaderboard.Sort((a, b) => b.progress.CompareTo(a.progress));

                // Draw cars
                i = 0;
                foreach (var entry in leaderboard)
                {
                    var car = entry.car;
                    var track = raceManager.CarTracks[car];
                    double progress = entry.progress;
                    double x = progress * (trackLength - 60) + 30;
                    double y = i * (carHeight + carSpacing) + 30;
                    // Bounce effect
                    double bounce = Math.Sin(Environment.TickCount / 100.0 + i) * 3;

                    // Draw car as an image from Assets using WPF Pack URI
                    string imageFile = "car_red.png";
                    if (car == raceManager.PlayerCar)
                        imageFile = "car_gold.png";
                    else if (i == 1)
                        imageFile = "car_red.png";
                    else if (i == 2)
                        imageFile = "car_blue.png";
                    else if (i == 3)
                        imageFile = "car_green.png";

                    try
                    {
                        var img = new Image
                        {
                            Width = 60,
                            Height = carHeight,
                            Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/{imageFile}"))
                        };
                        RaceCanvas.Children.Add(img);
                        Canvas.SetLeft(img, x);
                        Canvas.SetTop(img, y + bounce);
                    }
                    catch (Exception ex)
                    {
                        // Fallback: draw a rectangle if image fails
                        var rect = new Rectangle
                        {
                            Width = 60,
                            Height = carHeight,
                            Fill = Brushes.Gray,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            RadiusX = 10,
                            RadiusY = 10
                        };
                        RaceCanvas.Children.Add(rect);
                        Canvas.SetLeft(rect, x);
                        Canvas.SetTop(rect, y + bounce);
                    }

                    // Car name
                    var nameText = new TextBlock
                    {
                        Text = car.Name,
                        Foreground = Brushes.White,
                        FontWeight = car == raceManager.PlayerCar ? FontWeights.Bold : FontWeights.Normal,
                        FontSize = 14
                    };
                    RaceCanvas.Children.Add(nameText);
                    Canvas.SetLeft(nameText, 10);
                    Canvas.SetTop(nameText, y + 5 + bounce);

                    i++;
                }

                // Update leaderboard text
                LeaderboardText.Text = "Leaderboard: " + string.Join(" | ", leaderboard.Select((e, idx) => $"{idx + 1}. {e.car.Name} (Lap {e.lap})"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Draw error: {ex.Message}");
            }
        }

        private void ShowWinner()
        {
            // Play win sound
            try
            {
                string soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/win.wav");
                winPlayer.Open(new Uri(soundPath, UriKind.Absolute));
                winPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sound error: {ex.Message}");
            }
            if (raceManager.Winner == raceManager.PlayerCar)
                StatusText.Text = "🏁 You win! Congratulations!";
            else
                StatusText.Text = $"🏁 {raceManager.Winner.Name} wins the race!";
        }
    }
}