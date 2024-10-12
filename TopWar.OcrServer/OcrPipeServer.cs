// OcrServer
#pragma warning disable CA1416
using PaddleOCRSharp;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text.Json.Nodes;

public class OcrPipeServer
{
    //readonly string? _ServerId = null;  //这个是GameGUI+Id 加载游戏窗口开启的命名管道服务
    readonly string _pipeName = "OcrServer"; //本机提供OCR服务的命名管道名称
    readonly string _SharedMemoryExName = "SharedMemory";  //SharedMemory+Id  GameGUI窗口截图后存放bitmap的共享内存

    //启动和GameGUI通讯的命名管道
    readonly PersistentNamedPipeClient _client;

    readonly ConcurrentDictionary<int, Task> _clientTasks;
    readonly CancellationTokenSource _cts;
    readonly MemoryMappedFile _mmf;
    readonly MemoryMappedViewAccessor _accessor;
    //使用默认中英文V4模型
    readonly PaddleOCRSharp.OCRModelConfig? config = null;
    //使用默认参数
    readonly PaddleOCRSharp.OCRParameter oCRParameter = new();
    //oCRParameter.det_db_score_mode=true;
    //识别结果对象
    readonly PaddleOCRSharp.OCRResult ocrResult = new();
    readonly PaddleOCRSharp.PaddleOCREngine engine;
    public OcrPipeServer(string serverId)
    {
        //if (serverId == null) { Console.WriteLine("缺少serverId"); return; };
        _clientTasks = new ConcurrentDictionary<int, Task>();
        _cts = new CancellationTokenSource();
        _mmf = MemoryMappedFile.CreateOrOpen($"{_SharedMemoryExName}{serverId}", 5 * 1024 * 1024);
        _accessor = _mmf.CreateViewAccessor();

        //中英文模型V4
        config = new OCRModelConfig();
        string modelPathroot = @"C:\Users\Kash\Project\webview\Models";
        config.det_infer = modelPathroot + @"\ch_PP-OCRv4_det_server_infer";
        config.cls_infer = modelPathroot + @"\ch_ppocr_mobile_v2.0_cls_infer";
        config.rec_infer = modelPathroot + @"\ch_PP-OCRv4_rec_server_infer";
        config.keys = modelPathroot + @"\ppocr_keys.txt";
        //建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。     
        engine = new PaddleOCRSharp.PaddleOCREngine(config, oCRParameter);

        _client = new(serverId); //启动和游戏窗口通讯的命名管道(客户端，只是发送数据拿到结果)
    }

    public async Task StartAsync()
    {
        int clientId = 0;

        while (!_cts.IsCancellationRequested)
        {
            var pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            try
            {
                Console.WriteLine("等待客户端连接...");
                await pipeServer.WaitForConnectionAsync(_cts.Token);
                Console.WriteLine("客户端已连接。");

                var task = Task.Run(() => HandleClientAsync(pipeServer, clientId));

                _clientTasks.TryAdd(clientId, task);
                clientId++;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("服务器正在关闭...");
                break;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"管道连接时发生错误: {ex.Message}");
                await SendErrorToClientAsync(pipeServer, $"管道连接时发生错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生未处理的异常: {ex.Message}");
                await SendErrorToClientAsync(pipeServer, $"发生未处理的异常: {ex.Message}");
            }
        }
        //等待所有客户端任务完成
       await Task.WhenAll(_clientTasks.Values);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipeServer, int clientId)
    {
        try
        {
            using (var reader = new StreamReader(pipeServer))
            using (var writer = new StreamWriter(pipeServer) { AutoFlush = true })
            {
                while (!_cts.IsCancellationRequested)
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null) throw new Exception("OcrServer接受到的message为NULL");

                    JsonNode jsonNode = JsonNode.Parse(message)!;
                    var command = jsonNode["Command"]?.GetValue<string>() ?? "";

                    switch (command)
                    {
                        case "OCR":
                            await HandleOcrRequestAsync(jsonNode, writer);
                            break;
                        case "HEARTBEAT":
                            await writer.WriteLineAsync("OK");
                            break;
                        default:
                            await writer.WriteLineAsync("错误：未知命令");
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"客户端 {clientId} 处理时发生错误: {ex.Message}");
        }
        finally
        {
            pipeServer.Dispose();
            _clientTasks.TryRemove(clientId, out _);
            Console.WriteLine($"客户端 {clientId} 已断开连接。");
        }
    }
    private async Task HandleOcrRequestAsync(JsonNode jsonNode, StreamWriter writer)
    {
        //传入命令为 "O,请求对哪个窗口进行ocr,x1,y1,x2,y2
        try
        {
            // 命令,GameGUIServerId,x1,y1,x2,y2
            var gameGUIServerID = jsonNode["GameGUIServerID"]?.GetValue<int>() ?? 0;
            var x1 = jsonNode["x1"]?.GetValue<int>() ?? 0;
            var y1 = jsonNode["y1"]?.GetValue<int>() ?? 0;
            var x2 = jsonNode["x2"]?.GetValue<int>() ?? 0;
            var y2 = jsonNode["y2"]?.GetValue<int>() ?? 0;
            //先请求图像服务器截图 如果返回截图成功开始下面
            string received = await _client.SendMessageAsync("S");

            if (!string.IsNullOrEmpty(received))
            {
                int lengthByPipe = Convert.ToInt32(received);  //Pipe确定的长度
                int length = _accessor.ReadInt32(0);  // 读取图像数据的长度（假设是整数）
                if (length == lengthByPipe)
                {
                    byte[] buffer = new byte[length];

                    //读取内存中图像bitmap
                    _accessor.ReadArray(4, buffer, 0, length);

                    // 裁剪图像
                    byte[] croppedImageBytes = ImageProcessor.CropImage(buffer, x1, y1, x2, y2);

                    // 对裁剪后的图像进行OCR
                    var ocrResult = engine.DetectText(croppedImageBytes);
                    Console.WriteLine(ocrResult.JsonText);
                    await writer.WriteLineAsync(ocrResult.JsonText);
                    return;
                }
            }
            await writer.WriteLineAsync("OCRFAILED");
        }
        catch (Exception ex)
        {
            await writer.WriteLineAsync($"ERROR: HandleOcrRequestAsync: {ex.Message}");
        }
    }

    private async Task SendErrorToClientAsync(NamedPipeServerStream pipeServer, string errorMessage)
    {
        try
        {
            using (var writer = new StreamWriter(pipeServer) { AutoFlush = true })
            {
                await writer.WriteLineAsync($"ERROR:{errorMessage}");
            }
        }
        catch
        {
            Console.WriteLine($"无法向客户端发送错误消息: {errorMessage}");
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _ = _client.StopAsync();
    }

    public class ImageProcessor
    {
        public static byte[] CropImage(byte[] imageBytes, int x1, int y1, int x2, int y2)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                using (var originalImage = new Bitmap(ms))
                {
                    // 计算切割区域的宽度和高度
                    int width = x2 - x1;
                    int height = y2 - y1;

                    // 创建一个新的 Bitmap 来存储切割后的图像
                    using (var croppedImage = new Bitmap(width, height))
                    {
                        using (var g = Graphics.FromImage(croppedImage))
                        {
                            // 在新的 Bitmap 上绘制切割后的图像
                            g.DrawImage(originalImage, new Rectangle(0, 0, width, height), new Rectangle(x1, y1, width, height), GraphicsUnit.Pixel);
                        }

                        // 将切割后的图像转换为字节数组
                        using (var resultStream = new MemoryStream())
                        {
                            croppedImage.Save(resultStream, ImageFormat.Bmp);
                            return resultStream.ToArray();
                        }
                    }
                }
            }
        }
    }

}
#pragma warning restore CA1416
