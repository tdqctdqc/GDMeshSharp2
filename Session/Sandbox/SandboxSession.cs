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
        DoPoly();
    }

    private void DoTri()
    {
        var tri = new Triangle(new Vector2(-100f, -100f), new Vector2(100f, -100f), new Vector2(0f, 200f));
        Client.Setup(_home);
        Client.DrawTri(tri);
    }
    private void DoPoly()
    {
        var borderPoints = new List<Vector2>
        {

            new Vector2(-50f, -100f),
            new Vector2(50f, -100f),


            new Vector2(100f, -50f),
            // new Vector2(100f, 0f),
            new Vector2(100f, 50f),


            new Vector2(50f, 100f),
            // new Vector2(0f, 100f),
            new Vector2(-50f, 100f),


            new Vector2(-100f, 50f),
            // new Vector2(-100f, 0f),
            new Vector2(-100f, -50f),
        };
            
         var border = borderPoints   
            .GetLineSegments(true)
            .ToList();
         // border = border.OrderByClockwise(Vector2.Zero, ls => ls.From, border[0]).ToList();
             

        for (int i = 0; i < 10; i++)
        {
            var poly = new MockPolygon(Vector2.Zero,
                border,
                new List<Vector2>{
                    new Vector2(0f, -100f), 
                    new Vector2(100f, 0f), 
                    new Vector2(0f, 100f), 
                    new Vector2(-100f, 0f), 
                },
                new List<float>{20f, 
                    20f, 
                    20f, 
                    20f
                }, 
                0);
            

            if (i == 9)
            {
                Client.Setup(_home);
                Client.DrawPoly(poly);
            }
        }
        StopwatchMeta.DumpAll();

        
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