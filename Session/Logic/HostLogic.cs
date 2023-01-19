using Godot;
using System;

public class HostLogic : ILogic
{
    private HostServer _server;
    private HostWriteKey _key;
    private Data _data;
    public HostLogic()
    {
    }
    public void SetDependencies(HostServer server, Data data)
    {
        _data = data;
        _server = server;
        _key = new HostWriteKey(server, data);
    }

    public void ReceiveGenerationData(WorldData data)
    {
        
    }
}
