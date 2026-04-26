using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace EventTimerOverlay
{
    public partial class MainWindow : Window
    {
        private OverlayWindow overlay;
        private DispatcherTimer timer = new DispatcherTimer();
        private TimeSpan remaining, total;

        private OverlaySettings settings;
        private string screenKey;

        public MainWindow()
        {
            InitializeComponent();

            ScreenSelector.ItemsSource = Screen.AllScreens;
            ScreenSelector.SelectedIndex = Screen.AllScreens.Length > 1 ? 1 : 0;

            settings = SettingsManager.Load();

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Tick;
        }

        protected override void OnClosed(EventArgs e)
        {
            timer.Stop();
            overlay?.Close();

            base.OnClosed(e);
        }

        private void Tick(object s, EventArgs e)
        {
            remaining -= TimeSpan.FromMilliseconds(100);
            if (remaining <= TimeSpan.Zero) 
            { 
                remaining = TimeSpan.Zero; 
                timer.Stop();
                overlay?.Update(remaining, total);

                if (AutoHideCheckBox.IsChecked == true)
                {
                    // Show the final 0:00 state for a moment before hiding, then fade out.
                    var delay = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(5)
                    };

                    delay.Tick += (_, __) =>
                    {
                        delay.Stop();
                        overlay?.FadeOut();
                    };

                    delay.Start();
                }
                return;
            }

            overlay?.Update(remaining, total);
        }

        private void StartPreset(int min)
        {
            total = TimeSpan.FromMinutes(min);
            remaining = total;
            timer.Start();
            ShowOverlay();
        }

        private void Preset1_Click(object sender, RoutedEventArgs e)
        {
            StartPreset(1);
        }

        private void Preset2_Click(object sender, RoutedEventArgs e)
        {
            StartPreset(2);
        }

        private void Preset5_Click(object sender, RoutedEventArgs e)
        {
            StartPreset(5);
        }

        private void Preset10_Click(object sender, RoutedEventArgs e)
        {
            StartPreset(10);
        }

        private void Preset15_Click(object sender, RoutedEventArgs e)
        {
            StartPreset(15);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinutesInput.Text.Trim(), out int minutes) && minutes > 0)
            {
                StartPreset(minutes);
            }
            else
            {
                System.Windows.MessageBox.Show(this, "Please enter a positive whole number of minutes.", 
                    "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MinutesInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartButton_Click(sender, e);
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            EndTimer();
        }

        private void EndTimer()
        {
            // Stop timer immediately
            timer.Stop();

            // Update overlay one last time (so it doesn't freeze mid-count)
            overlay?.Update(remaining, total);

            // Fade out overlay
            overlay?.FadeOut();

            // Reset remaining time (optional)
            remaining = TimeSpan.Zero;
        }


        private void ShowOverlay_Click(object s, RoutedEventArgs e) => ShowOverlay();

        private void ShowOverlay()
        {
            var screen = GetCurrentScreen();
            screenKey = screen.DeviceName;

            if (overlay == null)
            {
                overlay = new OverlayWindow();
                overlay.PositionChanged += SavePosition;
            }

            if (settings.Positions.TryGetValue(screenKey, out var p))
            {
                VerticalCheckBox.IsChecked = p.IsVertical;
                overlay.SetOrientation(p.IsVertical);
                overlay.Left = p.Left;
                overlay.Top = p.Top;
                overlay.Width = p.Width;
                overlay.Height = p.Height;
            }
            else
            {
                overlay.Left = screen.Bounds.Left + 100;
                overlay.Top = screen.Bounds.Bottom - 120;
                overlay.Width = 600;
                overlay.Height = 80;
                overlay.SetOrientation(VerticalCheckBox.IsChecked == true);
            }

            overlay.SetClickThrough(EditModeCheckBox.IsChecked == false);
            overlay.Show();
            overlay.FadeIn();

            VerticalCheckBox.IsEnabled = (EditModeCheckBox.IsChecked == true);
        }

        private void HideOverlay_Click(object s, RoutedEventArgs e)
        {
            overlay?.FadeOut();
        }

        private void EditMode_Checked(object sender, RoutedEventArgs e)
        {
            EnableEditMode(true);
        }

        private void EditMode_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableEditMode(false);
            SaveCurrentLayout();
        }

        private void EnableEditMode(bool enable)
        {
            if (enable)
            {
                ShowOverlay();
                overlay?.SetClickThrough(false);
                EditModeCheckBox.IsChecked = true;
                VerticalCheckBox.IsEnabled = true;
            }
            else
            {
                overlay?.SetClickThrough(true);
                EditModeCheckBox.IsChecked = false;
                VerticalCheckBox.IsEnabled = false;
            }
        }

        private void SavePosition(double l, double t, double w, double h)
        {
            settings.Positions[screenKey] = new OverlayPosition
            {
                Left = l,
                Top = t,
                Width = w,
                Height = h,
                IsVertical = overlay.IsVertical
            };
            SettingsManager.Save(settings);
        }

        private void VerticalCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            overlay?.SetOrientation(true);
            SaveCurrentLayout();
        }

        private void VerticalCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            overlay?.SetOrientation(false);
            SaveCurrentLayout();
        }

        private void SaveCurrentLayout()
        {
            if (overlay == null) return;

            SavePosition(overlay.Left, overlay.Top, overlay.Width, overlay.Height);
        }

        private Screen GetCurrentScreen()
        {
            return (Screen)ScreenSelector.SelectedItem;
        }

        private void EnsureOverlay()
        {
            var screen = GetCurrentScreen();
            screenKey = screen.DeviceName;

            if (overlay == null)
            {
                overlay = new OverlayWindow();
                overlay.PositionChanged += SavePosition;
            }

            if (!overlay.IsVisible)
            {
                overlay.Show();
            }
        }

        private void LayoutBottom_Click(object sender, RoutedEventArgs e)
        {
            EnsureOverlay();
            EnableEditMode(false);
            var s = GetCurrentScreen();

            overlay.SetOrientation(false);
            VerticalCheckBox.IsChecked = false;

            overlay.Width = s.Bounds.Width * 0.75;
            overlay.Height = 50;

            overlay.Left = s.Bounds.Left + (s.Bounds.Width - overlay.Width) / 2;
            overlay.Top = s.Bounds.Bottom - overlay.Height;

            SavePosition(overlay.Left, overlay.Top, overlay.Width, overlay.Height);
        }

        private void LayoutTop_Click(object sender, RoutedEventArgs e)
        {
            EnsureOverlay();
            EnableEditMode(false);
            var s = GetCurrentScreen();

            overlay.SetOrientation(false);
            VerticalCheckBox.IsChecked = false;

            overlay.Width = s.Bounds.Width * 0.75;
            overlay.Height = 50;

            overlay.Left = s.Bounds.Left + (s.Bounds.Width - overlay.Width) / 2;
            overlay.Top = s.Bounds.Top;

            SavePosition(overlay.Left, overlay.Top, overlay.Width, overlay.Height);
        }

        private void LayoutMiddle_Click(object sender, RoutedEventArgs e)
        {
            EnsureOverlay();
            EnableEditMode(false);
            var s = GetCurrentScreen();

            overlay.SetOrientation(false);
            VerticalCheckBox.IsChecked = false;

            overlay.Width = s.Bounds.Width * 0.5;
            overlay.Height = 50;

            overlay.Left = s.Bounds.Left + (s.Bounds.Width - overlay.Width) / 2;
            overlay.Top = s.Bounds.Top + (s.Bounds.Height - overlay.Height) / 2;

            SavePosition(overlay.Left, overlay.Top, overlay.Width, overlay.Height);
        }

        private void LayoutRight_Click(object sender, RoutedEventArgs e)
        {
            EnsureOverlay();
            EnableEditMode(false);
            var s = GetCurrentScreen();

            overlay.SetOrientation(true);
            VerticalCheckBox.IsChecked = true;

            overlay.Width = 60;
            overlay.Height = s.Bounds.Height * 0.75;

            overlay.Left = s.Bounds.Right - overlay.Width;
            overlay.Top = s.Bounds.Top + (s.Bounds.Height - overlay.Height) / 2;

            SavePosition(overlay.Left, overlay.Top, overlay.Width, overlay.Height);
        }

        private void LayoutLeft_Click(object sender, RoutedEventArgs e)
        {
            EnsureOverlay();
            EnableEditMode(false);
            var s = GetCurrentScreen();

            overlay.SetOrientation(true);
            VerticalCheckBox.IsChecked = true;

            overlay.Width = 60;
            overlay.Height = s.Bounds.Height * 0.75;

            overlay.Left = s.Bounds.Left;
            overlay.Top = s.Bounds.Top + (s.Bounds.Height - overlay.Height) / 2;

            SavePosition(overlay.Left, overlay.Top, overlay.Width, overlay.Height);
        }
    }
}