using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public class Session : Node, ISession
{
    public Data Data { get; private set; }
    public IClient Client { get; private set; }
    private ILogic _logic;
    public UserCredential UserCredential { get; private set; }
    public override void _Process(float delta)
    {
        Client?.Process(delta);
    }

    public void StartAsHost(GenData data, UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        var hServer = new HostServer();
        var logic = new HostLogic();
        _logic = logic;
        data.ClearAuxData();
        Data = data;
        hServer.SetDependencies(logic, Data);
        logic.SetDependencies(hServer, Data);
        
        StartServer(hServer);
        StartClient(hServer);

        Game.I.Serializer.TestSerialization(Data, new HostWriteKey(hServer, data));
    }
    
    public void StartAsRemote(UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        
        _logic = new RemoteLogic();
        
        //todo fix this
        Data = new Data();
        var server = new RemoteServer();
        server.Setup(this, Data);
        StartServer(server);
        // StartClient(server);

    }

    private void SetCredential(UserCredential userCredential)
    {
        if (userCredential == null)
        {
            userCredential = new UserCredential("doot", "doot");
        }
        UserCredential = userCredential;
    }

    private void StartServer(IServer server)
    {
        ((Node)server).Name = "Server";
        AddChild((Node)server);
    }

    public void StartClient(IServer server, ServerWriteKey key)
    {
        StartClient(server);
    }
    private void StartClient(IServer server)
    {
        var client = new GameClient();
        Client = client;
        client.Setup(Data, server);
        AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        Client?.HandleInput(e, delta);
    }
}
