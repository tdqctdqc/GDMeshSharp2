
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class Game : Node
{
    public static Game I { get; private set; }
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    private ISession _session;
    public RefFulfiller RefFulfiller => _session.Data.RefFulfiller;
    public override void _Ready()
    {
        if (I != null)
        {
            throw new Exception();
        }
        I = this;

        Serializer.Setup();
        
    }

    public void OpenGenerator()
    {
        var genUi = GeneratorClient.Get();
        genUi.Setup();
        AddChild(genUi);
    }
    public void StartClientSession()
    {
        var session = new Session();
        _session = session;
        session.Name = "Session";   
        session.StartAsRemote();
        AddChild(session);
    }
    public void StartHostSession(WorldData data)
    {
        var session = new Session();
        _session = session;
        session.Name = "Session";   
        session.StartAsHost(data);
        AddChild(session);
    }
}
