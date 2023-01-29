using Godot;
using System;

public class TestCommand : Command
{
    public string Code { get; private set; }
    public static void Send(string scanCode, WriteKey key, IServer server)
    {
        var c = new TestCommand(scanCode, key);
        server.PushCommand(c);
    }

    private TestCommand(string code, WriteKey key) : base(key)
    {
        Code = code;
    }
    public override void Enact(HostWriteKey key)
    {
        GD.Print(Code);
    }
}
