
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class Game : Node
{
    public static Game I { get; private set; }
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    public ISession Session { get; private set; }
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
        Session = session;
        session.Name = "Session";   
        session.StartAsClient();
        AddChild(session);
    }
    public void StartHostSession(WorldData data)
    {
        var session = new Session();
        Session = session;
        session.Name = "Session";   
        session.StartAsHost(data);
        AddChild(session);
    }
}
