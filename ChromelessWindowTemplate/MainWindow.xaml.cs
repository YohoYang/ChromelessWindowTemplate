using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ChromelessWindowTemplate.API;

namespace ChromelessWindowTemplate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //参照http://blog.csdn.net/dlangu0393/article/details/12548731而成
        public MainWindow()
        {
            InitializeComponent();
            #region 无边框窗口相关监听事件
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.StateChanged += MainWindow_StateChanged;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            this.SizeChanged += MainWindow_SizeChanged;
            #endregion
        }
        #region 无边框窗口相关代码
        static int customBorderThickness;

        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
            }
            else
            {
                if (e.OriginalSource is Border || e.OriginalSource is TextBlock || e.OriginalSource is Grid || e.OriginalSource is Window)
                {
                    WindowInteropHelper wih = new WindowInteropHelper(this);
                    Win32.SendMessage(wih.Handle, Win32.WM_NCLBUTTONDOWN, (int)Win32.HitTest.HTCAPTION, 0);
                    return;
                }
            }
        }
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                ToggleMaximum.SetValue(Button.StyleProperty, Application.Current.Resources["K_Bbutton"]);
            }
            else
            {
                ToggleMaximum.SetValue(Button.StyleProperty, Application.Current.Resources["Kbutton"]);
            }
        }
        private void ToggleMaximum_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            customBorderThickness = int.Parse(windows_main.BorderThickness.Right.ToString());
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source == null)
                // Should never be null  
                throw new Exception("Cannot get HwndSource instance.");

            source.AddHook(new HwndSourceHook(this.WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case Win32.WM_GETMINMAXINFO: // WM_GETMINMAXINFO message  
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
                case Win32.WM_NCHITTEST: // WM_NCHITTEST message  
                    return WmNCHitTest(lParam, ref handled);
            }

            return IntPtr.Zero;
        }
        /// <summary>  
        /// Corner width used in HitTest  
        /// </summary>  
        private readonly int cornerWidth = customBorderThickness + 1;

        /// <summary>  
        /// Mouse point used by HitTest  
        /// </summary>  
        private Point mousePoint = new Point();

        private IntPtr WmNCHitTest(IntPtr lParam, ref bool handled)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                return IntPtr.Zero;
            }
            // Update cursor point  
            // The low-order word specifies the x-coordinate of the cursor.  
            // #define GET_X_LPARAM(lp) ((int)(short)LOWORD(lp))  
            this.mousePoint.X = (int)(short)(lParam.ToInt32() & 0xFFFF);
            // The high-order word specifies the y-coordinate of the cursor.  
            // #define GET_Y_LPARAM(lp) ((int)(short)HIWORD(lp))  
            this.mousePoint.Y = (int)(short)(lParam.ToInt32() >> 16);

            // Do hit test  
            handled = true;
            if (Math.Abs(this.mousePoint.Y - this.Top) <= this.cornerWidth
                && Math.Abs(this.mousePoint.X - this.Left) <= this.cornerWidth)
            { // Top-Left  
                return new IntPtr((int)Win32.HitTest.HTTOPLEFT);
            }
            else if (Math.Abs(this.ActualHeight + this.Top - this.mousePoint.Y) <= this.cornerWidth
                && Math.Abs(this.mousePoint.X - this.Left) <= this.cornerWidth)
            { // Bottom-Left  
                return new IntPtr((int)Win32.HitTest.HTBOTTOMLEFT);
            }
            else if (Math.Abs(this.mousePoint.Y - this.Top) <= this.cornerWidth
                && Math.Abs(this.ActualWidth + this.Left - this.mousePoint.X) <= this.cornerWidth)
            { // Top-Right  
                return new IntPtr((int)Win32.HitTest.HTTOPRIGHT);
            }
            else if (Math.Abs(this.ActualWidth + this.Left - this.mousePoint.X) <= this.cornerWidth
                && Math.Abs(this.ActualHeight + this.Top - this.mousePoint.Y) <= this.cornerWidth)
            { // Bottom-Right  
                return new IntPtr((int)Win32.HitTest.HTBOTTOMRIGHT);
            }
            else if (Math.Abs(this.mousePoint.X - this.Left) <= customBorderThickness)
            { // Left  
                return new IntPtr((int)Win32.HitTest.HTLEFT);
            }
            else if (Math.Abs(this.ActualWidth + this.Left - this.mousePoint.X) <= customBorderThickness)
            { // Right  
                return new IntPtr((int)Win32.HitTest.HTRIGHT);
            }
            else if (Math.Abs(this.mousePoint.Y - this.Top) <= customBorderThickness)
            { // Top  
                return new IntPtr((int)Win32.HitTest.HTTOP);
            }
            else if (Math.Abs(this.ActualHeight + this.Top - this.mousePoint.Y) <= customBorderThickness)
            { // Bottom  
                return new IntPtr((int)Win32.HitTest.HTBOTTOM);
            }
            else
            {
                handled = false;
                return IntPtr.Zero;
            }
        }
        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            // MINMAXINFO structure  
            Win32.MINMAXINFO mmi = (Win32.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Win32.MINMAXINFO));

            // Get handle for nearest monitor to this window  
            WindowInteropHelper wih = new WindowInteropHelper(this);
            IntPtr hMonitor = Win32.MonitorFromWindow(wih.Handle, Win32.MONITOR_DEFAULTTONEAREST);

            // Get monitor info  
            Win32.MONITORINFOEX monitorInfo = new Win32.MONITORINFOEX();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            Win32.GetMonitorInfo(new HandleRef(this, hMonitor), monitorInfo);

            // Get HwndSource  
            HwndSource source = HwndSource.FromHwnd(wih.Handle);
            if (source == null)
                // Should never be null  
                throw new Exception("Cannot get HwndSource instance.");
            if (source.CompositionTarget == null)
                // Should never be null  
                throw new Exception("Cannot get HwndTarget instance.");

            // Get transformation matrix  

            //取消了DPI相关转换
            //Matrix matrix = source.CompositionTarget.TransformFromDevice;

            // Convert working area  
            Win32.RECT workingArea = monitorInfo.rcWork;
            Point dpiIndependentSize =
                new Point(//注释DPI转换相关
                        workingArea.Right - workingArea.Left,
                        workingArea.Bottom - workingArea.Top
                        );
            //Point dpiIndependentSize =
            //    matrix.Transform(new Point(
            //            workingArea.Right - workingArea.Left,
            //            workingArea.Bottom - workingArea.Top
            //            ));

            // Convert minimum size  
            Point dpiIndenpendentTrackingSize = new Point(//取消DPI相关转换
                this.MinWidth,
                this.MinHeight
                );
            //Point dpiIndenpendentTrackingSize = matrix.Transform(new Point(
            //    this.MinWidth,
            //    this.MinHeight
            //    ));

            // Set the maximized size of the window  
            mmi.ptMaxSize.x = (int)dpiIndependentSize.X;
            mmi.ptMaxSize.y = (int)dpiIndependentSize.Y;

            // Set the position of the maximized window  
            mmi.ptMaxPosition.x = 0;
            mmi.ptMaxPosition.y = 0;

            // Set the minimum tracking size  
            mmi.ptMinTrackSize.x = (int)dpiIndenpendentTrackingSize.X;
            mmi.ptMinTrackSize.y = (int)dpiIndenpendentTrackingSize.Y;

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                customBorderThickness = int.Parse(windows_main.BorderThickness.Right.ToString());
                this.BorderThickness = new System.Windows.Thickness(0);
            }
            else
            {
                this.BorderThickness = new System.Windows.Thickness(customBorderThickness);
            }
        }
        #endregion
    }
}
