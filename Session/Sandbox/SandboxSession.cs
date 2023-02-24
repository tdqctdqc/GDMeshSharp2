using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SandboxSession : Node, ISession
{
    public RefFulfiller RefFulfiller => null;
    private Vector2 _home;
    IClient ISession.Client => Client;
    public SandboxClient Client { get;  set; }
    public override void _Ready()
    {
        var client = SceneManager.Instance<SandboxClient>();
        AddChild(client);
        _home = Vector2.Zero;
        Client = client;
        client.Setup(Vector2.Zero);
        new TriangulationTesting(Client).Run();
        OrToolsExt.Test();
    }

    public override void _Process(float delta)
    {
        Client?.ProcessPoly(delta);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        Client?.HandleInput(e, GetProcessDeltaTime());
    }
}