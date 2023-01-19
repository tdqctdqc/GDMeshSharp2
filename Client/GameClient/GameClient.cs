using Godot;
using System;

public class GameClient : Node, IClient
{
    private EntityOverview _entityOverview;
    private GameUi _ui;
    private IServer _server;
    public CameraController Cam { get; private set; }
    public Node2D GraphicsNode { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Setup(Data data, IServer server)
    {
        GraphicsNode = new Node2D();
        AddChild(GraphicsNode);

        _server = server;
        _entityOverview = EntityOverview.Get(data);
        AddChild(_entityOverview);
        _ui = GameUi.Get();
        _ui.Setup(server is HostServer, data);
        AddChild(_ui);

        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;
    }

    public void HandleInput(InputEvent e, float delta)
    {
        
    }

}
