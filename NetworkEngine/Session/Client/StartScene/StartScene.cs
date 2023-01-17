using Godot;
using System;

public class StartScene : Node
{
    private Button _hostBtn, _clientBtn;
    public override void _Ready()
    {
        _hostBtn = (Button)FindNode("Host");
        _hostBtn.Connect("button_up", this, nameof(StartAsHost));
        _clientBtn = (Button)FindNode("Client");
        _clientBtn.Connect("button_up", this, nameof(StartAsClient));
    }

    public void StartAsHost()
    {
        GD.Print("starting as host");
        Game.I.StartSession(true);
        QueueFree();
    }

    public void StartAsClient()
    {
        GD.Print("starting as client");
        Game.I.StartSession(false);
        QueueFree();
    }
}
