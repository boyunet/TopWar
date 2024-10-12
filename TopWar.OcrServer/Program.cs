// OcrServer
using System;
using System.Threading.Tasks;

InitializeAsync();

while (true)
{
    Console.WriteLine("Enter a command (start, stop, exit):");
    string? command = Console.ReadLine()?.Trim().ToLower();

    if (command == "start")
    {
        Console.WriteLine("Starting server and client...");
        var serverTask = Task.Run(() => Pipe.Server.StartAsync());
        //var clientTask = Task.Run(() => Pipe.Client.StartAsync());

    }
    else if (command == "stop")
    {
        Console.WriteLine("Stopping server and client...");
        Pipe.Server.Stop();
        //await Pipe.Client.StopAsync();
    }
    else if (command == "exit")
    {
        Console.WriteLine("Exiting...");
        break;
    }
    else
    {
        Console.WriteLine("Unknown command. Please enter 'start', 'stop', or 'exit'.");
    }
}

void InitializeAsync()
{
    // Initial setup if needed
    Task.Run(() => Pipe.Server.StartAsync());
    //Task.Run(() => Pipe.Client.StartAsync());
    //Task.Run(() => Socket.socket());
    //Task.Run(() => PipeTest.pipetest());
}

public partial class Pipe
{
    private static readonly OcrPipeServer _server = new("3661"); //本机
    //private static readonly PersistentNamedPipeClient _client = new("3661");  //游戏窗口

    public static OcrPipeServer Server => _server;
    //public static PersistentNamedPipeClient Client => _client;
}
