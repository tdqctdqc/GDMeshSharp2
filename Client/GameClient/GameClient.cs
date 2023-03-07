using Godot;
using System;

public class GameClient : Node, IClient
{
    private EntityOverview _entityOverview;
    public GameUi Ui { get; private set; }
    public GameSession Session { get; private set; }
    private IServer _server;
    public CameraController Cam { get; private set; }
    public GameGraphics Graphics { get; private set; }
    public Data Data { get; private set; }
    public ClientWriteKey Key { get; private set; }
    public override void _Ready()
    {
        
    }

    public void Process(float delta)
    {
        if (GetParent() == null) return;
        Graphics?.Process(delta, Data);
        Ui?.Process(delta, Key);
    }
    public void Setup(GameSession session, IServer server)
    {
        Key = new ClientWriteKey(session.Data, session);
        Session = session;
        Data = Session.Data;
        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;
        
        BuildGraphics(Session.Data);
        BuildUi(Session.Data, Key.Session.Server);
        Graphics.Setup(this, Session.Data);
    }
    
    private void BuildGraphics(Data data)
    {
        Graphics = GameGraphics.Get();
        Graphics.Setup(this, data);
        AddChild(Graphics);
    }

    private void BuildUi(Data data, IServer server)
    {
        Ui = SceneManager.Instance<GameUi>();
        Ui.Setup(server is HostServer, data, Graphics, Cam, this);
        _server = server;
        _entityOverview = EntityOverview.Get(data);
        AddChild(_entityOverview);
        AddChild(Ui);
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }

}
