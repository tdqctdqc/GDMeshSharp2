
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Game : Node
{
    public static Game I { get; private set; }
    public Serializer Serializer { get; private set; }
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    private ISession _session;

    public RefFulfiller RefFulfiller => _session == null
        ? _genUi.Data.RefFulfiller
        : _session.Data.RefFulfiller;
    //todo fix this
    private GeneratorClient _genUi;
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

    public override void _Process(float delta)
    {
        _genUi?.Process(delta);
    }

    public void OpenGenerator()
    {
        _genUi = SceneManager.Instance<GeneratorClient>();
        _genUi.Setup();
        AddChild(_genUi);
    }
    public void StartClientSession()
    {
        var session = new Session();
        _session = session;
        session.Name = "Session";   
        session.StartAsRemote();
        AddChild(session);
    }
    public void StartHostSession(GenData data)
    {
        var session = new Session();
        _session = session;
        session.Name = "Session";   
        session.StartAsHost(data);
        AddChild(session);
        RemoveChild(_genUi);
        _genUi.QueueFree();
        _genUi = null;
    }
}
