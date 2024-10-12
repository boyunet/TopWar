using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using TopWar.GameGUI.ImageProcessing;
using TopWar.GameGUI.libs;
using TopWar.GameGUI.Messaging;
using TopWar.GameGUI.Ocr;
using TopWar.GameGUI.Pipe;
using TopWar.GameGUI.ShareMemoryS;

namespace TopWar.GameGUI
{
    public class 服务器配置
    {
        public string? Server { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public string? Url { get; set; }

    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static 服务器配置 cfg = null!;
        public ChromiumWebBrowser? chromeBrowser;
        //public static dmsoft Dm { get; set; } = new dmsoft();
        public MainWindow()
        {
            //debug
            InitializeComponent();
            cfg = 读取本地服务器配置()!;
            if (cfg == null)
                Environment.Exit(0);

            InitializeChromium();

            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing!);
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            //this.ResizeMode = ResizeMode.NoResize;
            this.Left = cfg.Left;
            this.Top = cfg.Top;
            this.Title = cfg.Server;

            //IntPtr ptr = ChromiumWebBrowserIns.GetWindowHandle;获取句柄
            ChromiumWebBrowserIns browser = new(chromeBrowser!);

            //存放截图
            IShareMemory shareMemory = new ShareMemory(cfg.Server!);

            // 创建 OcrService 实例
            IOcrService ocrService = new OcrService();
            ImageProcessor imageProcessor = new(shareMemory);

            // 创建 MessageHandler 实例，注入 OcrService
            IMessageHandler messageHandler = new MessageHandler(ocrService, imageProcessor);

            string pipeName = $"GameGUI{cfg.Server}";
            PipeServer pipeServer = new PipeServer(pipeName, messageHandler);

            Task.Run(()=>pipeServer.StartAsync().Wait());

            //var server = new NamedPipeServer(this, chromeBrowser!, cfg.Server!);
            //Task.Run(() => server.StartAsync());

        }
        public static 服务器配置? 读取本地服务器配置()
        {
            try
            {
                string jsonString = File.ReadAllText("setting.json");
                服务器配置? config = JsonConvert.DeserializeObject<服务器配置>(jsonString);
                return config;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"读取配置文件时出错: {ex.Message}");
                MessageBox.Show($"读取配置文件时出错: {ex.Message}");
                return null;
            }
        }
        private void InitializeChromium()
        {
            // 初始化 CefSettings
            CefSettings settings = new()
            {
                CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"CEF{cfg.Server}")
            };
            //settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
            Cef.Initialize(settings);
            // 初始化 ChromiumWebBrowser
            chromeBrowser = new ChromiumWebBrowser(cfg.Url!);
            ((System.Windows.Controls.Panel)this.Content).Children.Add(chromeBrowser);
            chromeBrowser.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            chromeBrowser.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            this.Width = 1296;
            this.Height = 799;

        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cef.Shutdown();
        }


    }
}