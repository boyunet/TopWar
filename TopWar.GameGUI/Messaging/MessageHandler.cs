using CefSharp;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Interop;
using TopWar.GameGUI.ImageProcessing;
using TopWar.GameGUI.libs;
using TopWar.GameGUI.Ocr;
using TopWar.GameGUI.ShareMemoryS;

namespace TopWar.GameGUI.Messaging
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IOcrService _ocrService;
        private readonly IImageProcessor _imageProcessor;

        public MessageHandler(IOcrService ocrService, IImageProcessor imageProcessor)
        {
            _ocrService = ocrService;
            _imageProcessor = imageProcessor;
        }

        public async Task HandleMessageAsync(JsonNode jsonNode, StreamWriter writer)
        {
            var command = jsonNode["Command"]?.GetValue<string>() ?? "";
            //Debug.WriteLine(command);

            switch (command)
            {
                case "HEARTBEAT":
                    await writer.WriteLineAsync("OK");
                    break;
                case "OCR":
                    await HandleOcrRequestAsync(jsonNode, writer);
                    break;
                case "SCREENSHOT32BIT":
                    await HandleScreenShot32RequestAsync(jsonNode, writer);
                    break;
                case "SCREENSHOT24BIT":
                    await HandleScreenShot24RequestAsync(jsonNode, writer);
                    break;
                case "CLICK":
                    await HandleClickRequestAsync(jsonNode, writer);
                    break;
                case "MOVE":
                    await HandleMoveRequestAsync(jsonNode, writer);
                    break;
                case "SCROLL":
                    await HandleScroolRequestAsync(jsonNode, writer);
                    break;
                case "KEYPRESS":
                    await HandleKeyPressRequestAsync(jsonNode, writer);
                    break;
                case "HANDLE":
                    IntPtr handle = ChromiumWebBrowserIns.GetWindowHandle;
                    await writer.WriteLineAsync($"{handle}");
                    break;
                default:
                    await writer.WriteLineAsync("错误：未知命令");
                    break;
            }
        }

        private async Task HandleOcrRequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            var x1 = jsonNode["x1"]?.GetValue<int>() ?? 0;
            var y1 = jsonNode["y1"]?.GetValue<int>() ?? 0;
            var x2 = jsonNode["x2"]?.GetValue<int>() ?? 0;
            var y2 = jsonNode["y2"]?.GetValue<int>() ?? 0;
            var imageBytes = ImageProcessor.GetBitmapData(760, 1280, true);
            var cropeImageData = ImageProcessor.CropImage(imageBytes!, x1, y1, x2, y2);
            var result = _ocrService.PerformOcrAsync(cropeImageData);
            await writer.WriteLineAsync(result);
        }

        private async Task HandleScreenShot32RequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            
            int length = _imageProcessor.WriteToSharedMemory(true);
            await writer.WriteLineAsync($"{length}");
        }
        private async Task HandleScreenShot24RequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            int length = _imageProcessor.WriteToSharedMemory(false);
            await writer.WriteLineAsync($"{length}");
        }
        private async Task HandleClickRequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            var x = jsonNode["x"]?.GetValue<int>() ?? 0;
            var y = jsonNode["y"]?.GetValue<int>() ?? 0;
            bool success = await PerformClickAsync(x, y);
            await writer.WriteLineAsync(success ? "CLICK_SUCCESS" : "CLICK_FAILURE");
        }
        private async Task HandleMoveRequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            var x = jsonNode["x"]?.GetValue<int>() ?? 0;
            var y = jsonNode["y"]?.GetValue<int>() ?? 0;
            bool success = await PerformMoveAsync(x, y);
            await writer.WriteLineAsync(success ? "MOVE_SUCCESS" : "MOVE_FAILURE");
        }
        private async Task HandleScroolRequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            var x = jsonNode["x"]?.GetValue<int>() ?? 0;
            var y = jsonNode["y"]?.GetValue<int>() ?? 0;
            var deltaX = jsonNode["deltaX"]?.GetValue<int>() ?? 0;
            var deltaY = jsonNode["deltaY"]?.GetValue<int>() ?? 0;
            bool success = await PerformScrollAsync(x, y, deltaX, deltaY);
            await writer.WriteLineAsync(success ? "SCROOL_SUCCESS" : "SCROOL_FAILURE");
        }
        private async Task HandleKeyPressRequestAsync(JsonNode jsonNode, StreamWriter writer)
        {
            var keyCode = jsonNode["keyCode"]?.GetValue<int>() ?? 0;
            bool success = await PerformKeyPressAsync(keyCode);
            await writer.WriteLineAsync(success ? "KeyPress_SUCCESS" : "KeyPress_FAILURE");
        }
        private Task<bool> PerformClickAsync(int x, int y)
        {
            // 实现点击逻辑
            //throw new NotImplementedException();
            SimulateMouseClick(x, y, MouseButtonType.Left);
            return Task.FromResult(true);
        }
        private Task<bool> PerformMoveAsync(int x, int y)
        {
            // 实现点击逻辑
            //throw new NotImplementedException();
            SimulateMoveMouseTo(x, y);
            return Task.FromResult(true);
        }
        private Task<bool> PerformScrollAsync(int x, int y, int deltaX, int deltaY)
        {
            // 实现点击逻辑
            //throw new NotImplementedException();
            SimulateMouseScroll(x, y, deltaX, deltaY);
            return Task.FromResult(true);
        }
        private Task<bool> PerformKeyPressAsync(int keyCode)
        {
            // 实现点击逻辑
            //throw new NotImplementedException();
            SimulateKeyPress(keyCode);
            return Task.FromResult(true);
        }

        private void SimulateMouseClick(int x, int y, MouseButtonType buttonType)
        {
            var host = ChromiumWebBrowserIns.GetBrowser().GetBrowser().GetHost();
            host.SendMouseClickEvent(x, y, buttonType, false, 1, CefEventFlags.None); // 按下
            host.SendMouseClickEvent(x, y, buttonType, true, 1, CefEventFlags.None);  // 松开
        }
        private void SimulateMoveMouseTo(int x, int y)
        {
            var host = ChromiumWebBrowserIns.GetBrowser().GetBrowser().GetHost();
            var mouseEvent = new MouseEvent(x, y, CefEventFlags.None);
            host.SendMouseMoveEvent(mouseEvent, false);
        }
        private void SimulateMouseScroll(int x, int y, int deltaX, int deltaY)
        {
            var host = ChromiumWebBrowserIns.GetBrowser().GetBrowser().GetHost();
            host.SendMouseWheelEvent(x, y, deltaX, deltaY, CefEventFlags.None);
            //0, 0 是鼠标事件的坐标位置，表示事件发生在窗口的左上角。
            //deltaX 和 deltaY 分别表示鼠标滚轮在水平方向和垂直方向上的滚动量。
            //具体来说：
            //deltaX：表示水平方向的滚动量。正值表示向右滚动，负值表示向左滚动。
            //deltaY：表示垂直方向的滚动量。正值表示向下滚动，负值表示向上滚动。
        }
        private void SimulateKeyPress(int keyCode)
        {
            var host = ChromiumWebBrowserIns.GetBrowser().GetBrowser().GetHost();
            var keyEvent = new KeyEvent
            {
                WindowsKeyCode = keyCode,
                Type = KeyEventType.KeyDown
            };
            host.SendKeyEvent(keyEvent);

            keyEvent.Type = KeyEventType.KeyUp;
            host.SendKeyEvent(keyEvent);
        }

        //示例:
        //var browser = new ChromiumWebBrowser("https://www.example.com");

        // 模拟鼠标点击
        //SimulateMouseClick(browser, 100, 100, MouseButtonType.Left);

        // 模拟鼠标移动
        //SimulateMouseMove(browser, 200, 200);

        // 模拟鼠标滚动
        //SimulateMouseScroll(browser, 0, 120); // 向下滚动

        // 模拟键盘按键
        //SimulateKeyPress(browser, Keys.A); // 按下 'A' 键

    }
}
