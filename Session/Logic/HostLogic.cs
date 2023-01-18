using Godot;
using System;

public class HostLogic : ILogic
{
    private HostServer _server;
    private HostWriteKey _key;

    public HostLogic()
    {
        _key = new HostWriteKey();
    }
    public void SetDependencies(HostServer server)
    {
        _server = server;
    }

    public void ReceiveGenerationData(WorldData data)
    {
        
    }
}
