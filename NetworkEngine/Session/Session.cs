using Godot;
using System;

public class Session : Node, ISession
{
    public Data Data { get; private set; }
    private IClient _client;
    public UserCredential UserCredential { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Start(bool isHost, UserCredential userCredential = null)
    {
        if (userCredential == null)
        {
            userCredential = new UserCredential("doot", "doot");
        }
        UserCredential = userCredential;
        IServer server;
        if (isHost)
        {
            var hServer = new HostServer();
            server = hServer;
            var logic = new HostLogic();
            hServer.SetDependencies(logic);
            logic.SetDependencies(hServer);
            hServer.Name = "Server";
            AddChild(hServer);
        }
        else
        {
            var cServer = new ClientServer();
            server = cServer;
            cServer.Name = "Server";
            AddChild(cServer);
        }
        
        Data = new Data(server);
        var client = new ExampleClient();
        _client = client;
        client.Setup(server);
        AddChild((Node)_client);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        _client.HandleInput(e, delta);
    }
}
