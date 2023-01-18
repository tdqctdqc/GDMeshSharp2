using Godot;
using System;

public class TestCommand : ICommand
{
    public static void Send(string scanCode, IServer server)
    {
        server.ReceiveCommand(nameof(TestCommand), scanCode);
    }
    
    public static void DeserializeAndEnact(string json, HostWriteKey key)
    {
        GD.Print(json);
    }
}
