
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Game : Node
{
    public static Game I { get; private set; }
    public Serializer Serializer { get; private set; }
    public Guid PlayerGuid { get; private set; } = Guid.NewGuid();
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    private ISession _session;
    public Action NewSession { get; set; }

    public RefFulfiller RefFulfiller => _session.RefFulfiller;
    public override void _Ready()
    {
        if (I != null)
        {
            throw new Exception();
        }
        I = this;
        SceneManager.Setup();
        Serializer = new Serializer();
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
    public void StartHostSession(GenData data)
    {
        var session = new GameSession();
        SetSession(session);
        session.StartAsHost(data);
    }

    public void StartSandbox()
    {
        SetSession(new SandboxSession());
    }

    private void SetSession(Node session)
    {
        _session?.QueueFree();
        session.Name = "Session";
        _session = (ISession)session;
        AddChild(session);
        NewSession?.Invoke();
    }
}
