using Godot;
using System;

public class GameClient : Node, IClient
{
    private EntityOverviewWindow _entityOverviewWindow;
    public GameUi Ui { get; private set; }
    private IServer _server;
    public ClientRequests Requests { get; private set; }
    public TooltipManager TooltipManager => Ui.TooltipManager;
    public ICameraController Cam { get; private set; }
    public MapGraphics Graphics { get; private set; }
    public Data Data { get; private set; }
    public ClientWriteKey Key { get; private set; }
    public ClientSettings Settings { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Process(float delta)
    {
        if (GetParent() == null) return;
        Graphics?.Process(delta);
        Ui?.Process(delta, Key);
        TooltipManager?.Process(delta, Cam.GetMousePosInMapSpace());
    }
    public void Setup(GameSession session, IServer server, MapGraphics graphics)
    {
        Requests = new ClientRequests(session);
        Requests.GiveTree(session.Data.EntityTypeTree);
        Settings = ClientSettings.Load();
        Key = new ClientWriteKey(session.Data, session);
        Data = session.Data;
        var cam = WorldCameraController.Construct(Data);
        AddChild(cam);
        cam.Current = true;
        Cam = cam;

        if (graphics == null)
        {
            BuildGraphics(session.Data);
        }
        else
        {
            Graphics = graphics;
        }
        AddChild(Graphics);
        BuildUi(session.Data, Key.Session.Server);
    }
    
    private void BuildGraphics(Data data)
    {
        Graphics = new MapGraphics();
        Graphics.Setup(data);
    }

    private void BuildUi(Data data, IServer server)
    {
        Ui = GameUi.Construct(this, server is HostServer, data, Graphics);
        AddChild(Ui);
        _server = server;
        _entityOverviewWindow = EntityOverviewWindow.Get(data);
        AddChild(_entityOverviewWindow);
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }
}
