using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public class GameSession : Node, ISession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    public Data Data { get; private set; }
    public IClient Client { get; private set; }
    private ILogic _logic;
    private IServer _server;
    public UserCredential UserCredential { get; private set; }
    public override void _Process(float delta)
    {
        _logic?.Process(delta);
        Client?.Process(delta);
    }
    
    public void StartAsHost(GenData data, UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        var hServer = new HostServer();
        _server = hServer;
        var logic = new HostLogic();
        _logic = logic;
        data.ClearAuxData();
        Data = data;
        hServer.SetDependencies(logic, Data);
        logic.SetDependencies(hServer, Data);
        StartServer(hServer);
        StartClient(hServer);
    }
    
    public void StartAsRemote(UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        
        Data = new Data();
        Data.Setup();
        var logic = new RemoteLogic(Data);
        _logic = logic;
        var server = new RemoteServer();
        _server = server;
        Data.Notices.FinishedStateSync += () => StartClient(server);
        server.Setup(this, logic, Data);
        StartServer(server);
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

    private void StartClient(IServer server)
    {
        var client = new GameClient();
        Client = client;
        client.Setup(this, server);
        AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        Client?.HandleInput(e, delta);
    }

    public void TestSerialization()
    {
        if(_server is HostServer h) Game.I.Serializer.TestSerialization(new HostWriteKey(h, Data));
    }
}
