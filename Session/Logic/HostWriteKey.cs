using Godot;
using System;

public class HostWriteKey : CreateWriteKey
{
    public HostServer HostServer { get; private set; }
    public HostWriteKey(HostServer hostServer, Data data) : base(data)
    {
        HostServer = hostServer;
    }
}
