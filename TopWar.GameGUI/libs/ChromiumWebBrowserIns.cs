using CefSharp.Wpf;
using System.Windows.Interop;
using System.Windows;

namespace TopWar.GameGUI.libs
{
    public class ChromiumWebBrowserIns
    {
        private static ChromiumWebBrowser? _browser;
        public ChromiumWebBrowserIns(ChromiumWebBrowser browser)
        {
            _browser = browser;
        }
        public static ChromiumWebBrowser GetBrowser()
        {
            return _browser!;
        }
        public static IntPtr GetWindowHandle
        {
            get
            {
                // 获取 ChromiumWebBrowser 的句柄
                //var windowInteropHelper = new WindowInteropHelper(Window.GetWindow(_browser));
                //IntPtr browserHandle = windowInteropHelper.Handle;

                Window mainWindow = Application.Current.MainWindow;
                WindowInteropHelper helper = new(mainWindow);
                IntPtr handle = helper.Handle;
                return handle;
            }
        }
    }
}
