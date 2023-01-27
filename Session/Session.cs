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
    
    public void StartAsHost(WorldData data, UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        var hServer = new HostServer();
        var logic = new HostLogic();
        _logic = logic;
        data.ClearAuxData();
        Data = data;
        hServer.SetDependencies(logic, Data);
        logic.SetDependencies(hServer, Data);
        
        // var sw = new Stopwatch();
        // sw.Start();
        // var stateTransfer = StateTransferUpdate.Encode(new HostWriteKey(hServer, data)).Serialize();
        // sw.Stop();
        // GD.Print("state transfer building time " + sw.Elapsed.Seconds);
        // GD.Print("state transfer size " + System.Text.ASCIIEncoding.Unicode.GetByteCount(stateTransfer) / 1_000_000f);
        //
        StartServer(hServer);
        StartClient(hServer);

        var firstDom = Data.Domains.First().Value;
        var firstE = Data.Entities.First().Value;
        var firstEBytes = Game.I.Serializer.SerializeToUtf8(firstE);
        var firstE2 = Game.I.Serializer.Deserialize(firstEBytes, firstE.GetType());
    }
    
    public void StartAsRemote(UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        
        _logic = new RemoteLogic();
        
        //todo fix this
        Data = new Data();
        var server = new RemoteServer();
        server.Setup(Data);
        StartServer(server);
        StartClient(server);

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
        // var client = new GameClient();
        // Client = client;
        // client.Setup(Data, server);
        // AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        Client?.HandleInput(e, delta);
    }
}
