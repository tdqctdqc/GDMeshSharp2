using Godot;
using System;

public class GameClient : Node, IClient
{
    private EntityOverviewWindow _entityOverviewWindow;
    public GameUi Ui { get; private set; }
    private IServer _server;
    public CameraController Cam { get; private set; }
    public GameGraphics Graphics { get; private set; }
    public Data Data { get; private set; }
    public ClientWriteKey Key { get; private set; }
    public ClientSettings Settings { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Process(float delta)
    {
        if (GetParent() == null) return;
        Graphics?.Process(delta, Data);
        Ui?.Process(delta, Key);
    }
    public void Setup(GameSession session, IServer server, GameGraphics graphics)
    {
        Settings = ClientSettings.Load();
        Key = new ClientWriteKey(session.Data, session);
        Data = session.Data;
        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;
        
        BuildUi(session.Data, Key.Session.Server);

        if (graphics == null)
        {
            BuildGraphics(session.Data);
        }
        else
        {
            Graphics = graphics;
            Graphics.GetParent().RemoveChild(Graphics);
            Graphics.SetClient(this);
        }
        AddChild(Graphics);
    }
    
    private void BuildGraphics(Data data)
    {
        Graphics = GameGraphics.Get();
        Graphics.SetClient(this);
        Graphics.Setup(data);
    }

    private void BuildUi(Data data, IServer server)
    {
        Ui = SceneManager.Instance<GameUi>();
        Ui.Setup(server is HostServer, data, Graphics, Cam, this);
        _server = server;
        _entityOverviewWindow = EntityOverviewWindow.Get(data);
        AddChild(_entityOverviewWindow);
        AddChild(Ui);
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }

}
