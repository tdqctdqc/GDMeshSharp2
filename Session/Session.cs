using Godot;
using System;

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
        hServer.SetDependencies(logic);
        logic.SetDependencies(hServer);
        StartServer(hServer);
        StartClient(hServer);
    }
    
    public void StartAsClient(UserCredential userCredential = null)
    {
        SetCredential(userCredential);
        var server = new ClientServer();
        _logic = new ClientLogic();
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
        Data = new Data(server);
    }
    private void StartClient(IServer server)
    {
        var client = new GameClient();
        Client = client;
        client.Setup(server);
        AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = GetProcessDeltaTime();
        Client.HandleInput(e, delta);
    }
}
