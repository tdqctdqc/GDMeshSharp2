using Godot;
using System;

public class GameClient : Node, IClient
{
    private EntityOverview _entityOverview;
    public GameUi Ui { get; private set; }
    private IServer _server;
    public CameraController Cam { get; private set; }
    public GameGraphics Graphics { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Setup(Data data, IServer server)
    {
        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;
        
        BuildGraphics(data);
        BuildUi(data, server);
        if (server is RemoteServer r)
        {
            r.ReceivedStateTransfer += () => Graphics.Setup(this, data, Cam);
        }
    }

    private void BuildGraphics(Data data)
    {
        Graphics = GameGraphics.Get();
        Graphics.Setup(this, data, Cam);
        AddChild(Graphics);
    }

    private void BuildUi(Data data, IServer server)
    {
        Ui = GameUi.Get();
        Ui.Setup(server is HostServer, data, Graphics);
        _server = server;
        _entityOverview = EntityOverview.Get(data);
        AddChild(_entityOverview);
        AddChild(Ui);
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }

}
