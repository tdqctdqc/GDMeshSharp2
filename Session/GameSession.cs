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
    public IServer Server { get; private set; }
    public UserCredential UserCredential { get; private set; }
    private WriteKey _key;
    public override void _Process(float delta)
    {
        _logic?.Process(delta);
        Client?.Process(delta);
    }

    public void Setup()
    {
        
    }
    public void StartAsHost(GenData data, MapGraphics graphics = null)
    {
        Data = data;
        var hServer = new HostServer();
        Server = hServer;
        var logic = new HostLogic();
        _logic = logic;
        var hKey = new HostWriteKey(hServer, logic, data, this);
        _key = hKey;

        data.ClearAuxData();
        hServer.SetDependencies(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        Player.Create(Game.I.PlayerGuid, "Doot", hKey);
        StartClient(hServer, graphics);
    }
    
    public void StartAsRemote()
    {
        Data = new Data();
        Data.Setup();

        var logic = new RemoteLogic(Data, this);
        _logic = logic;
        var server = new RemoteServer();
        Server = server;
        _key = new WriteKey(Data, this);

        Data.Notices.FinishedStateSync.Subscribe(() =>
        {
            StartClient(server, null);
        });
        server.Setup(this, logic, Data);
        StartServer(server);
    }

    private void StartServer(IServer server)
    {
        ((Node)server).Name = "Server";
        AddChild((Node)server);
    }

    private void StartClient(IServer server, MapGraphics graphics)
    {
        var client = new GameClient();
        Client = client;
        client.Setup(this, server, graphics);
        AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        Client?.HandleInput(e, delta);
    }
}
