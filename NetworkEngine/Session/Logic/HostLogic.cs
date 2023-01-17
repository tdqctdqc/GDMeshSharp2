using Godot;
using System;

public class HostLogic
{
    private HostServer _server;
    public void SetDependencies(HostServer server)
    {
        _server = server;
    }
}
