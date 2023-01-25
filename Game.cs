
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class Game : Node
{
    public static Game I { get; private set; }
    public Serializer Serializer { get; private set; }
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    private ISession _session;

    public RefFulfiller RefFulfiller => _session == null
        ? _tempData.Data.RefFulfiller
        : _session.Data.RefFulfiller;
    //todo fix this
    private GeneratorClient _tempData;
    public override void _Ready()
    {
        if (I != null)
        {
            throw new Exception();
        }
        I = this;

        Serializer = new Serializer();
    }

    public void OpenGenerator()
    {
        var genUi = GeneratorClient.Get();
        _tempData = genUi;

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
