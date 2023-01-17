using Godot;
using System;

public class ExampleClient : Node, IClient
{
    private EntityOverview _entityOverview;
    private ExampleUi _ui;
    private IServer _server;
    public override void _Ready()
    {
        
    }

    public void Setup(IServer server)
    {
        _server = server;
        _entityOverview = EntityOverview.Get();
        AddChild(_entityOverview);
        _ui = ExampleUi.Get();
        _ui.Setup(server is HostServer);
        AddChild(_ui);
    }

    public void HandleInput(InputEvent e, float delta)
    {
        if (e is InputEventKey k)
        {
            TestCommand.Send(k.Scancode.ToString(), _server);
        }
    }
}
