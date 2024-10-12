//  OcrServer && ImageServer 公用
using System.IO.Pipes;

public class PersistentNamedPipeClient
{
    private readonly string _pipeName;
    readonly string _PipeServerExName = "GameGUI";  //GameGUI+Id 向游戏窗口(通过命名管道)请求截图并返回截图大小
    private NamedPipeClientStream? _pipeClient = null;
    private StreamReader? _reader = null;
    private StreamWriter? _writer = null;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private Task? _heartbeatTask = null;

    public bool IsConnected => _pipeClient?.IsConnected ?? false;
    private const int HEARTBEAT_INTERVAL = 5000; // 5 seconds

    public PersistentNamedPipeClient(string serverId)
    {
        _pipeName = $"{_PipeServerExName}{serverId}";
    }

    public Task StartAsync()
    {
        _heartbeatTask = HeartbeatAsync();
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        if (_heartbeatTask != null)
        {
            await _heartbeatTask;
        }
        await DisconnectAsync();
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (!IsConnected)
        {
            await ConnectAsync();
        }

        if (_writer == null || _reader == null)
        {
            return string.Empty;
        }

        try
        {
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
            string response = await _reader.ReadLineAsync() ?? string.Empty;
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessage: {ex.Message}");
            await DisconnectAsync();
            return string.Empty;
        }
    }
    private async Task ConnectAsync()
    {
        int retryCount = 3;
        while (retryCount > 0)
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipeClient.ConnectAsync(_cts.Token);
                _reader = new StreamReader(_pipeClient);
                _writer = new StreamWriter(_pipeClient) { AutoFlush = true };
                Console.WriteLine($"Connected to pipe: {_pipeName}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                await DisconnectAsync();
                retryCount--;
                if (retryCount > 0)
                {
                    await Task.Delay(1000); // 等待一秒后重试
                }
            }
        }
    }

    private async Task DisconnectAsync()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        if (_pipeClient != null)
        {
            await _pipeClient.DisposeAsync();
        }
        _pipeClient = null;
        _reader = null;
        _writer = null;
        Console.WriteLine("Disconnected");
    }
    private async Task HeartbeatAsync()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                string response = await SendMessageAsync("HEARTBEAT");
                if (response != "OK")
                {
                    Console.WriteLine("Heartbeat failed. Reconnecting...");
                    await DisconnectAsync();
                    await ConnectAsync();
                }
                await Task.Delay(HEARTBEAT_INTERVAL, _cts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // 任务被取消，正常退出
            Console.WriteLine("Heartbeat task was canceled.");
        }
        catch (Exception ex)
        {
            // 处理其他可能的异常
            Console.WriteLine($"Unexpected error in HeartbeatAsync: {ex.Message}");
        }
    }

}