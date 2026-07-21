using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;

namespace Gemini
{
    public partial class MainWindow : Window
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const uint WDA_NONE = 0x00000000;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int WM_GETMINMAXINFO = 0x0024;

        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_TRANSIENTWINDOW = 3;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        private const int HOTKEY_ID = 9000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_G = 0x47;

        public MainWindow()
        {
            try { SetCurrentProcessExplicitAppUserModelID("Morgoth.GeminiDesktop.Unofficial.v7"); } catch { }

            InitializeComponent();
            LoadWindowIcon();
            InitializeWebView();
            this.StateChanged += MainWindow_StateChanged;
        }

        private void LoadWindowIcon()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string icoPath = Path.Combine(baseDir, "app.ico");
                string pngPath = Path.Combine(baseDir, "4e92ffe9-5767-4708-a3a6-6282763449b7.png");

                if (File.Exists(icoPath))
                {
                    BitmapImage icoImage = new BitmapImage();
                    icoImage.BeginInit();
                    icoImage.UriSource = new Uri(icoPath, UriKind.Absolute);
                    icoImage.CacheOption = BitmapCacheOption.OnLoad;
                    icoImage.EndInit();
                    this.Icon = icoImage;
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                
                if (File.Exists(pngPath))
                {
                    bitmap.UriSource = new Uri(pngPath, UriKind.Absolute);
                }
                else if (File.Exists(icoPath))
                {
                    bitmap.UriSource = new Uri(icoPath, UriKind.Absolute);
                }

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                AppIcon.Source = bitmap;
            }
            catch { }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                MainBorder.CornerRadius = new CornerRadius(0);
                MainBorder.BorderThickness = new Thickness(0);
                btnMax.Content = "❏";
            }
            else
            {
                MainBorder.CornerRadius = new CornerRadius(8);
                MainBorder.BorderThickness = new Thickness(1);
                btnMax.Content = "□";
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2) { ToggleMaximize(); }
                else { this.DragMove(); }
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnMaximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void ToggleMaximize()
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnBackToGemini_Click(object sender, RoutedEventArgs e) => ReturnToGemini();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { ReturnToGemini(); }
        }

        private void ReturnToGemini()
        {
            if (webView != null) { webView.Source = new Uri("https://gemini.google.com"); }
        }

        private async void InitializeWebView()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string portableDataFolder = Path.Combine(baseDir, "Data", "Profile");

                if (!Directory.Exists(portableDataFolder))
                {
                    Directory.CreateDirectory(portableDataFolder);
                }

                var env = await CoreWebView2Environment.CreateAsync(null, portableDataFolder);
                await webView.EnsureCoreWebView2Async(env);

                webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;

                string escKeyJs = @"
                    (function() {
                        window.addEventListener('keydown', function(e) {
                            if (e.key === 'Escape' || e.keyCode === 27) {
                                if (window.location.href.includes('accounts.google.com')) {
                                    window.location.href = 'https://gemini.google.com';
                                }
                            }
                        }, true);
                    })();
                ";

                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(escKeyJs);

                webView.CoreWebView2.NavigationStarting += (s, args) =>
                {
                    if (args.Uri.StartsWith("http://")) { args.Cancel = true; }
                };

                webView.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        string currentUrl = webView.Source.ToString();
                        if (currentUrl.Contains("myaccount.google.com") || currentUrl.Contains("accounts.google.com/CheckCookie"))
                        {
                            webView.Source = new Uri("https://gemini.google.com");
                        }
                    }
                };

                webView.CoreWebView2.NewWindowRequested += (s, e) =>
                {
                    if (e.Uri.Contains("accounts.google.com") || e.Uri.Contains("google.com"))
                    {
                        e.Handled = true;
                        webView.Source = new Uri(e.Uri);
                    }
                    else
                    {
                        e.Handled = true;
                    }
                };

                webView.CoreWebView2.PermissionRequested += (s, args) => { args.State = CoreWebView2PermissionState.Allow; };
                webView.Source = new Uri("https://gemini.google.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur WebView2 : {ex.Message}", "Gemini", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            try
            {
                var helper = new WindowInteropHelper(this);
                int backdropType = DWMSBT_TRANSIENTWINDOW;
                DwmSetWindowAttribute(helper.Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
                SetWindowDisplayAffinity(helper.Handle, WDA_NONE);

                HwndSource source = HwndSource.FromHwnd(helper.Handle);
                source.AddHook(HwndHook);
                RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_G);
            }
            catch { }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
            }
            else if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                GetMonitorInfo(monitor, ref monitorInfo);

                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;

                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void ToggleWindow()
        {
            if (this.IsVisible && this.WindowState != WindowState.Minimized && this.IsActive)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.WindowState = WindowState.Maximized;
                this.Activate();
                this.Focus();
            }
        }
    }
}
