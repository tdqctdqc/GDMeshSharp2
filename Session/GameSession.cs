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
    
    public void StartAsHost(GenData data)
    {
        Data = data;
        var hServer = new HostServer();
        Server = hServer;
        var logic = new HostLogic();
        _logic = logic;
        _key = new HostWriteKey(hServer, logic, data, this);

        data.ClearAuxData();
        hServer.SetDependencies(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        CreatePlayer(_key);
        StartClient(hServer);
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

        Data.Notices.FinishedStateSync += () =>
        {
            CreatePlayer(_key);
            StartClient(server);
        };
        server.Setup(this, logic, Data);
        StartServer(server);
        
    }

    private void CreatePlayer(WriteKey key)
    {
        if (key is HostWriteKey h)
        {
            GD.Print("creating player");
            Player.Create(h.IdDispenser.GetID(), Game.I.PlayerGuid, "Doot", h);
            GD.Print(Data.BaseDomain.Players.Entities.Count);
            GD.Print(Data.BaseDomain.Players.LocalPlayer == null);
        }
        else
        {
            var regime = Data.Society.Regimes.Entities.Where(r => r.IsPlayerRegime(Data) == false).First();
            var com = CreatePlayerCommand.Construct(Game.I.PlayerGuid, "Doot");
            Server.QueueCommandLocal(com, _key);
        }
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
        if(Server is HostServer h) 
            Game.I.Serializer.TestSerialization(new HostWriteKey(h, null, Data, this));
    }
}
