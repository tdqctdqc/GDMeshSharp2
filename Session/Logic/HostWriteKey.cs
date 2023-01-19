using Godot;
using System;

public class HostWriteKey : StrongWriteKey
{
    public HostServer Server { get; private set; }
    public HostWriteKey(HostServer server, Data data) : base(data)
    {
        Server = server;
    }
}
