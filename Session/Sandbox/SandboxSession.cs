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
        
        var poly = new MockPolygon(Vector2.Zero,
            new List<LineSegment>
            {
                new LineSegment(new Vector2(-500f, -500f), new Vector2(500f, -500f)),
                new LineSegment(new Vector2(500f, -500f), new Vector2(500f, 500f)),
                new LineSegment(new Vector2(500f, 500f), new Vector2(-500f, 500f)),
                new LineSegment(new Vector2(-500f, 500f), new Vector2(-500f, -500f)),
            },
            new List<Vector2>{new Vector2(0f, -500f)},
            new List<float>{20f}, 0);
        client.Setup(_home);
        Client.DrawPoly(poly);
    }
    

    public override void _Process(float delta)
    {
        Client?.Process(delta);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        Client?.HandleInput(e, GetProcessDeltaTime());
    }
}