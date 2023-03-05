using Godot;
using System;

public class HostWriteKey : CreateWriteKey
{
    public HostServer HostServer { get; private set; }
    public HostLogic Logic { get; private set; }
    public HostWriteKey(HostServer hostServer, HostLogic logic, Data data) : base(data)
    {
        Logic = logic;
        HostServer = hostServer;
    }
}
