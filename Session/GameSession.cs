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
    public UserCredential UserCredential { get; private set; }
    public override void _Process(float delta)
    {
        _logic?.Process(delta);
        Client?.ProcessPoly(delta);
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
    }
    public void StartAsTest(UserCredential userCredential = null)
    {
        var worldGen = new WorldGenerator(new GenerationParameters(new Vector2(8000f, 4000f)));
        var data = worldGen.Generate();
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
        var key = new HostWriteKey(hServer, Data);
        var msg = new MessageManager(u => { GD.Print(u.GetType()); }, p => { }, c => { });
        var e = Data.Planet.Polygons.Entities.First();
        var update = EntityCreationUpdate.GetForTest(e, key);
        var msg1 = msg.WrapUpdate(update);
        msg.HandleIncoming(msg1);
    }
    public void StartAsRemote(UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        
        
        //todo fix this
        Data = new Data();
        Data.Setup();
        var logic = new RemoteLogic(Data);
        _logic = logic;
        var server = new RemoteServer();
        Data.Notices.FinishedStateSync += () => StartClient(server);
        server.Setup(this, logic, Data);
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
