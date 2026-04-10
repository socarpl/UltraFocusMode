using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace UltraFocusMode
{
    public partial class CaptureWindow : Window
    {
        public event Action<IntPtr, NativeMethods.RECT>? WindowSelected;
        public event Action? Cancelled;

        public CaptureWindow()
        {
            InitializeComponent();

            // Span all monitors (virtual desktop)
            Left   = SystemParameters.VirtualScreenLeft;
            Top    = SystemParameters.VirtualScreenTop;
            Width  = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Focus();   // needed so KeyDown reaches this window
        }

        private async void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Record screen position BEFORE hiding the window
            var screenPos = PointToScreen(e.GetPosition(this));

            // Hide so that WindowFromPoint sees what is actually below
            Hide();

            // Give Windows one frame to process the hide
            await Task.Delay(50);

            var pt   = new NativeMethods.POINT { X = (int)screenPos.X, Y = (int)screenPos.Y };
            var hwnd = NativeMethods.WindowFromPoint(pt);
            var root = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOT);

            var desktop = NativeMethods.GetDesktopWindow();
            if (root == IntPtr.Zero || root == desktop || !NativeMethods.IsWindowVisible(root))
            {
                Cancelled?.Invoke();
                return;
            }

            NativeMethods.GetWindowRect(root, out var rect);
            WindowSelected?.Invoke(root, rect);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Cancelled?.Invoke();
            }
        }
    }
}
