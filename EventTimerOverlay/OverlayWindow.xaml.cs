using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Screen = System.Windows.Forms.Screen;
using Color = System.Windows.Media.Color;

namespace EventTimerOverlay
{
    public partial class OverlayWindow : Window
    {
        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int i, int v);

        private bool _clickThrough = true;
        private const int SNAP = 20;
        private bool _isVertical = false;

        public bool IsVertical => _isVertical;

        public event Action<double, double, double, double> PositionChanged;

        public OverlayWindow()
        {
            InitializeComponent();

            Loaded += (_, __) => ApplyClickThrough(true);
            LocationChanged += (_, __) => RaiseChanged();
            SizeChanged += (_, __) => RaiseChanged();
        }

        public void Update(TimeSpan remaining, TimeSpan total)
        {
            TimerText.Text = remaining.ToString(@"m\:ss");

            double p = total.TotalSeconds == 0 ? 0 :
                remaining.TotalSeconds / total.TotalSeconds;

            var color = new SolidColorBrush(GetColor(p));

            if (_isVertical)
            {
                VerticalBar.Height = ActualHeight * p;
                VerticalBar.Background = color;
            }
            else
            {
                HorizontalBar.Width = ActualWidth * p;
                HorizontalBar.Background = color;
            }
        }
        private Color GetColor(double p)
        {
            if (p > 0.5)
                return Lerp(Colors.Yellow, Colors.LimeGreen, (p - 0.5) * 2);
            else
                return Lerp(Colors.Red, Colors.Yellow, p * 2);
        }

        private Color Lerp(Color a, Color b, double t)
        {
            return Color.FromRgb(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t));
        }

        public void FadeIn()
        {
            BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)));
        }

        public void FadeOut()
        {
            BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)));
        }

        public void SetClickThrough(bool enabled)
        {
            _clickThrough = enabled;
            ApplyClickThrough(enabled);
        }

        private void ApplyClickThrough(bool enable)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hwnd, -20);

            if (enable)
                SetWindowLong(hwnd, -20, style | 0x80000 | 0x20);
            else
                SetWindowLong(hwnd, -20, style & ~0x20);
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_clickThrough)
                DragMove();
        }

        private void RaiseChanged()
        {
            if (!_clickThrough)
            {
                Snap();
                PositionChanged?.Invoke(Left, Top, Width, Height);
            }
        }

        private void Snap()
        {
            var screen = System.Windows.Forms.Screen.FromHandle(
                new WindowInteropHelper(this).Handle);

            var bounds = screen.Bounds;

            double centerX = bounds.Left + (bounds.Width / 2);
            double centerY = bounds.Top + (bounds.Height / 2);

            double windowCenterX = Left + (Width / 2);
            double windowCenterY = Top + (Height / 2);

            // Edge snapping (existing)
            if (Math.Abs(Left - bounds.Left) < SNAP) Left = bounds.Left;
            if (Math.Abs(Top - bounds.Top) < SNAP) Top = bounds.Top;
            if (Math.Abs((Left + Width) - bounds.Right) < SNAP) Left = bounds.Right - Width;
            if (Math.Abs((Top + Height) - bounds.Bottom) < SNAP) Top = bounds.Bottom - Height;

            // NEW: horizontal center snap
            if (Math.Abs(windowCenterX - centerX) < SNAP)
                Left = centerX - (Width / 2);

            // OPTIONAL: vertical center snap
            if (Math.Abs(windowCenterY - centerY) < SNAP)
                Top = centerY - (Height / 2);
        }


        public void SetOrientation(bool vertical)
        {
            _isVertical = vertical;

            HorizontalBar.Visibility = vertical ? Visibility.Collapsed : Visibility.Visible;
            VerticalBar.Visibility = vertical ? Visibility.Visible : Visibility.Collapsed;

            // Adjust default size when switching
            if (vertical)
            {
                Width = 60;
                Height = 300;
            }
            else
            {
                Width = 600;
                Height = 80;
            }
        }
    }
}