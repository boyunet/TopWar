// OcrServer
#pragma warning disable CA1416
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text.Json.Nodes;
using dm;

public class ImagePipeServer
{
    readonly string _pipeName = "ImageServer"; //本机提供OCR服务的命名管道名称
    readonly string _SharedMemoryExName = "SharedMemory";  //SharedMemory+Id  GameGUI窗口截图后存放bitmap的共享内存

    //启动和GameGUI通讯的命名管道
    readonly PersistentNamedPipeClient _client;

    readonly ConcurrentDictionary<int, Task> _clientTasks;
    readonly CancellationTokenSource _cts;
    readonly MemoryMappedFile _mmf;
    readonly MemoryMappedViewAccessor _accessor;
    readonly dmsoft dm = new();
    public ImagePipeServer(string serverId)
    {
        //if (serverId == null) { Console.WriteLine("缺少serverId"); return; };
        _clientTasks = new ConcurrentDictionary<int, Task>();
        _cts = new CancellationTokenSource();
        _mmf = MemoryMappedFile.CreateOrOpen($"{_SharedMemoryExName}{serverId}", 5 * 1024 * 1024);
        _accessor = _mmf.CreateViewAccessor();

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
                    if (message == null) break;

                    switch (message[0])
                    {
                        case 'F':
                            await HandleFindPicRequestAsync(message, writer);
                            break;
                        case 'H':
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
    private async Task HandleFindPicRequestAsync(string message, StreamWriter writer)
    {
        //传入命令为 "O,请求对哪个窗口进行ocr,x1,y1,x2,y2
        try
        {
            // 命令,GameGUIServerId,x1,y1,x2,y2
            var coordinates = message.Substring(1).Split(',');
            Console.WriteLine(coordinates);
            string GameGUIServerId = coordinates[0];
            int x1 = int.Parse(coordinates[1]);
            int y1 = int.Parse(coordinates[2]);
            int x2 = int.Parse(coordinates[3]);
            int y2 = int.Parse(coordinates[4]);
            Console.WriteLine($"GameGUI{GameGUIServerId}");
            //先请求图像服务器截图 如果返回截图成功开始下面
            string received = await _client.SendMessageAsync("S");

            Console.WriteLine(received);
            if (!string.IsNullOrEmpty(received))
            {
                int lengthByPipe = Convert.ToInt32(received);  //Pipe确定的长度
                int length = _accessor.ReadInt32(0);  // 读取图像数据的长度（假设是整数）
                Console.WriteLine($"send{lengthByPipe} men{length}");
                if (length == lengthByPipe)
                {
                    byte[] buffer = new byte[length];

                    //读取内存中图像bitmap
                    _accessor.ReadArray(4, buffer, 0, length);

                    // 裁剪图像
                    //byte[] croppedImageBytes = ImageProcessor.CropImage(buffer, x1, y1, x2, y2);
                    dm.FindPic(x1, y1, x2, y2, "croppedImageBytes", "000000", 0.7, 0, out object x, out object y);
                    dm.findpi
                    // 对裁剪后的图像进行  返回 结果 例如:dm_ret,x,y
                    //var ocrResult = engine.DetectText(croppedImageBytes);
                    //Console.WriteLine(ocrResult.JsonText);
                    await writer.WriteLineAsync("xxxxxxx");
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
