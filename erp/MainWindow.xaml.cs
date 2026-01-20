using erp.Services;
using erp.ViewModels.Returns;
using erp.Views;
using erp.Views.Category;
using erp.Views.Dashboard;
using erp.Views.Expenses;
using erp.Views.Invoices;
using erp.Views.Returns;
using erp.Views.Users;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace erp
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;

        // ====== ✅ Manual Maximize (Borderless + Transparency safe) ======
        private bool _isMaximized;
        private Rect _restoreBounds;

        public MainWindow()
        {
            InitializeComponent();

            // ✅ افتح على أكبر حجم يناسب الشاشة اللي اتفتح عليها البرنامج
            SourceInitialized += (_, __) =>
            {
                MoveToMouseMonitorWorkArea();

                // خزن مقاس الريستور قبل التكبير
                _restoreBounds = new Rect(Left, Top, Width, Height);

                ApplyWorkAreaToCurrentMonitor();
                _isMaximized = true;
            };

            // ====== باقي كودك ======
            var httpClient = ApiClient.CreateHttpClient();
            _apiClient = new ApiClient(httpClient, null);

            Services.NavigationService.Initialize(MainFrame);
            NavigateToDashboard();
        }

        // ================== Handlers المطلوبين من XAML ==================

        private void Logo_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException?.Message ?? "Image failed");
        }

        private void Sidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double click toggles maximize
            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); } catch { }
            }
        }

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem is not ListBoxItem selectedItem)
                return;

            var tag = selectedItem.Tag as string;
            if (string.IsNullOrWhiteSpace(tag))
                return;

            switch (tag)
            {
                case "Dashboard":
                    NavigateToDashboard();
                    break;

                case "Users":
                    NavigateToUsersPage();
                    break;

                case "Inventory":
                    MainFrame.Navigate(new erp.Views.Inventory.InventoryPage());
                    break;

                case "Invoices":
                    MainFrame.Navigate(new InvoicesListPage());
                    break;

                case "Expenses":
                    MainFrame.Navigate(new ExpensesListPage());
                    break;

                case "Items":
                    MainFrame.Navigate(new CategoryListPage());
                    break;

                case "Returns":
                    {
                        var returnsService = new ReturnsService(_apiClient);

                        var returnsVm = new ReturnsOrderItemsViewModel(returnsService);
                        var inventoryService = new InventoryService();
                        var createReturnVm = new CreateReturnViewModel(returnsService, inventoryService);

                        MainFrame.Navigate(new ReturnsOrderItemsPage(returnsVm, createReturnVm));
                        break;
                    }

                case "Reports":
                    MainFrame.Navigate(new erp.Views.Reports.SalesReportPage());
                    break;

                case "Orders":
                    MainFrame.Navigate(new erp.Views.Orders.ApprovedOrdersPage());
                    SelectNavItem("Orders");
                    break;

                case "Cheques":
                    MainFrame.Navigate(new erp.Views.Cheques.ChequesListPage());
                    break;

                default:
                    ShowUnderDevelopment(tag);
                    break;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Min_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Max_Click(object sender, RoutedEventArgs e)
            => ToggleMaximizeRestore();

        // ================== Navigation Methods ==================

        public void NavigateToDashboard()
        {
            MainFrame.Navigate(new DashboardPage());
            SelectNavItem("Dashboard");
        }

        public void NavigateToUsersPage()
        {
            MainFrame.Navigate(new AllUsersPage());
            SelectNavItem("Users");
        }

        private static void ShowUnderDevelopment(string tag)
        {
            string pageName = tag switch
            {
                "Dashboard" => "الداشبورد",
                "Orders" => "الطلبات",
                "Suppliers" => "الموردين",
                "Auth" => "المصادقة",
                _ => "الصفحة"
            };

            MessageBox.Show($"صفحة {pageName} قيد التطوير", "تطوير");
        }

        private void SelectNavItem(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                NavListBox.SelectedItem = null;
                return;
            }

            foreach (ListBoxItem item in NavListBox.Items)
            {
                if ((item.Tag as string) == tag)
                {
                    NavListBox.SelectedItem = item;
                    return;
                }
            }
        }

        // ================== ✅ Manual Maximize / Restore ==================

        private void ToggleMaximizeRestore()
        {
            if (!_isMaximized)
            {
                _restoreBounds = new Rect(Left, Top, Width, Height);
                ApplyWorkAreaToCurrentMonitor();
                _isMaximized = true;
            }
            else
            {
                RestoreFromManualMaximize();
                _isMaximized = false;
            }
        }

        private void RestoreFromManualMaximize()
        {
            WindowState = WindowState.Normal;

            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;

            Left = _restoreBounds.Left;
            Top = _restoreBounds.Top;
            Width = _restoreBounds.Width;
            Height = _restoreBounds.Height;
        }

        private void MoveToMouseMonitorWorkArea()
        {
            if (!GetCursorPos(out POINT pt))
                return;

            IntPtr hMon = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            if (hMon == IntPtr.Zero)
                return;

            RECT work = GetMonitorWorkArea(hMon);
            Rect dip = PixelRectToDip(work);

            WindowState = WindowState.Normal;

            Left = dip.Left;
            Top = dip.Top;

            Width = Math.Min(1200, dip.Width);
            Height = Math.Min(700, dip.Height);
        }

        private void ApplyWorkAreaToCurrentMonitor()
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            IntPtr hMon = hwnd != IntPtr.Zero
                ? MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST)
                : IntPtr.Zero;

            if (hMon == IntPtr.Zero)
            {
                if (!GetCursorPos(out POINT pt)) return;
                hMon = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
                if (hMon == IntPtr.Zero) return;
            }

            RECT work = GetMonitorWorkArea(hMon);
            Rect dip = PixelRectToDip(work);

            WindowState = WindowState.Normal;

            Left = dip.Left;
            Top = dip.Top;
            Width = dip.Width;
            Height = dip.Height;

            MaxWidth = dip.Width;
            MaxHeight = dip.Height;
        }

        private Rect PixelRectToDip(RECT px)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null)
                return new Rect(px.Left, px.Top, px.Right - px.Left, px.Bottom - px.Top);

            var m = source.CompositionTarget.TransformFromDevice;
            var tl = m.Transform(new Point(px.Left, px.Top));
            var br = m.Transform(new Point(px.Right, px.Bottom));

            return new Rect(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
        }

        private static RECT GetMonitorWorkArea(IntPtr hMon)
        {
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (!GetMonitorInfo(hMon, ref mi))
                return new RECT();

            return mi.rcWork; // WorkArea بدون Taskbar
        }

        // ================== Win32 ==================

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    }
}
