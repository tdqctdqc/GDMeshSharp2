using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public class GameSession : Node, IDataSession
{
    RefFulfiller ISession.RefFulfiller => Data.RefFulfiller;
    public Data Data { get; private set; }
    public IClient Client => _client;
    private GameClient _client;
    private ILogic _logic;
    public IServer Server { get; private set; }
    public UserCredential UserCredential { get; private set; }
    private WriteKey _key;
    public override void _Process(float delta)
    {
        if (_logic != null)
        {
            var gameStateChanged = _logic.Process(delta);
            Client?.Process(delta, gameStateChanged);
        }
    }

    public void Setup()
    {
        
    }
    public void StartAsHost(GenData data, MapGraphics graphics = null)
    {
        Data = data;
        Data.ClientPlayerData.SetLocalPlayerGuid(new Guid());
        var hServer = new HostServer();
        Server = hServer;
        var logic = new HostLogic(Data);
        _logic = logic;
        var hKey = new HostWriteKey(hServer, logic, data, this);
        _key = hKey;

        data.ClearAuxData();
        hServer.SetDependencies(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        Player.Create(_key.Data.ClientPlayerData.LocalPlayerGuid, "Doot", hKey);
        StartClient(hServer, graphics);
        _client.Ui.Prompts.PushPrompt(Prompt.GetChooseRegimePrompt(r => logic.Start(), hKey));
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
        _client = new GameClient();
        _client.Setup(this, server, graphics);
        AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        Client?.HandleInput(e, delta);
    }
}
