using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace UltraFocusMode
{
    public partial class OverlayWindow : Window
    {
        private readonly IntPtr      _targetHwnd;
        private readonly AppSettings _settings;
        private NativeMethods.RECT   _lastRect;

        private readonly DispatcherTimer _tracker;

        public event Action? Dismissed;

        public OverlayWindow(IntPtr hwnd, AppSettings settings)
        {
            InitializeComponent();

            _targetHwnd = hwnd;
            _settings   = settings;

            Left   = SystemParameters.VirtualScreenLeft;
            Top    = SystemParameters.VirtualScreenTop;
            Width  = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            _tracker = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _tracker.Tick += OnTrackerTick;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Show the correct exit combo in the hint
            EscHintText.Text = _settings.GetExitDisplayString() + " — exit focus mode";

            RebuildHole();
            _tracker.Start();

            Opacity = 0;
            BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
        }

        // ─── Hole geometry ───────────────────────────────────────────────────────

        private void RebuildHole()
        {
            NativeMethods.GetWindowRect(_targetHwnd, out var r);
            _lastRect = r;

            double vLeft = SystemParameters.VirtualScreenLeft;
            double vTop  = SystemParameters.VirtualScreenTop;
            double vW    = SystemParameters.VirtualScreenWidth;
            double vH    = SystemParameters.VirtualScreenHeight;

            // GetWindowRect returns physical (device) pixels.
            // WPF geometry lives in logical units (DIPs).
            var src = PresentationSource.FromVisual(this);
            double sx = src?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
            double sy = src?.CompositionTarget?.TransformFromDevice.M22 ?? 1.0;

            double hx = r.Left   * sx - vLeft;
            double hy = r.Top    * sy - vTop;
            double hw = r.Width  * sx;
            double hh = r.Height * sy;

            // Snap hole edges to whole device pixels
            double pw = 1.0 / sx;
            double ph = 1.0 / sy;
            hx = Math.Floor  (hx / pw) * pw;
            hy = Math.Floor  (hy / ph) * ph;
            hw = Math.Ceiling(hw / pw) * pw;
            hh = Math.Ceiling(hh / ph) * ph;

            hw = Math.Max(pw, hw);
            hh = Math.Max(ph, hh);

            var screen = new RectangleGeometry(new Rect(0, 0, vW, vH));
            var hole   = new RectangleGeometry(new Rect(hx, hy, hw, hh));

            OverlayPath.Data = new CombinedGeometry(GeometryCombineMode.Exclude, screen, hole);

            byte alpha = (byte)Math.Round(_settings.OverlayOpacity * 255);
            OverlayPath.Fill = new SolidColorBrush(Color.FromArgb(alpha, 0, 0, 0));
        }

        private void OnTrackerTick(object? sender, EventArgs e)
        {
            if (!NativeMethods.IsWindowVisible(_targetHwnd) || NativeMethods.IsIconic(_targetHwnd))
            {
                Dismiss();
                return;
            }

            NativeMethods.GetWindowRect(_targetHwnd, out var r);
            if (r.Left != _lastRect.Left || r.Top != _lastRect.Top ||
                r.Right != _lastRect.Right || r.Bottom != _lastRect.Bottom)
            {
                RebuildHole();
            }
        }

        // ─── Dismiss (public so App can call it directly) ────────────────────────

        public void Dismiss()
        {
            _tracker.Stop();

            Dispatcher.Invoke(() =>
            {
                var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
                anim.Completed += (_, _) =>
                {
                    Dismissed?.Invoke();
                    Close();
                };
                BeginAnimation(OpacityProperty, anim);
            });
        }
    }
}
