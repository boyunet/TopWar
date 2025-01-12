using System.Collections.Concurrent;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text.Json.Nodes;
using TopWar.GameGUI.Messaging;
using TopWar.GameGUI.ShareMemoryS;

namespace TopWar.GameGUI.Pipe
{
    public class PipeServer
    {
        private readonly string _pipeName;
        private readonly ConcurrentDictionary<int, Task> _clientTasks;
        private readonly CancellationTokenSource _cts;
        private readonly IMessageHandler _messageHandler;

        public PipeServer(string pipeName, IMessageHandler messageHandler)
        {
            _pipeName = pipeName;
            _clientTasks = new ConcurrentDictionary<int, Task>();
            _cts = new CancellationTokenSource();
            _messageHandler = messageHandler;
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
            // 实现处理单个客户端连接的逻辑
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

                        await _messageHandler.HandleMessageAsync(jsonNode, writer);

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
        }
    }
}
