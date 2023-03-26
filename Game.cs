
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Game : Node
{
    public static Game I { get; private set; }
    public Serializer Serializer { get; private set; }
    public Logger Logger { get; private set; }
    public Guid PlayerGuid { get; private set; } = Guid.NewGuid();
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    private ISession _session;

    public RefFulfiller RefFulfiller => _session.RefFulfiller;
    public IClient Client => _session.Client;
    public override void _Ready()
    {
        if (I != null)
        {
            throw new Exception();
        }
        I = this;
        Logger = new Logger();
        AssetManager.Setup();
        Serializer = new Serializer();
        StartMainMenuSession();
    }

    public void StartMainMenuSession()
    {
        SetSession(new MainMenuSession());
    }
    public void StartGeneratorSession()
    {
        SetSession(new GeneratorSession());
    }
    public void StartClientSession()
    {
        var session = new GameSession();
        SetSession(session);
        session.StartAsRemote();

    }
    public void StartHostSession(GenData data, MapGraphics graphics = null)
    {
        var session = new GameSession();
        SetSession(session);
        session.StartAsHost(data, graphics);
    }

    public void StartSandbox()
    {
        SetSession(new SandboxSession());
    }

    private void SetSession(Node session)
    {
        if(_session != null) RemoveChild((Node) _session);
        _session?.QueueFree();
        session.Name = "Session";
        _session = (ISession)session;
        _session.Setup();
        AddChild(session);
    }

    public void SetPlayerGuid(Guid guid, WriteKey key)
    {
        PlayerGuid = guid;
    }
}
