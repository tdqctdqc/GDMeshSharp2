using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class StartMenu : Node
{
    private ButtonToken _hostBtn, _clientBtn;
    public override void _Ready()
    {
        _hostBtn = ButtonToken.Get(this, "Host", StartAsHost); 
        _clientBtn = ButtonToken.Get(this, "Client", StartAsClient); 
    }

    public void StartAsHost()
    {
        Game.I.OpenGenerator();
        QueueFree();
    }

    public void StartAsClient()
    {
        Game.I.StartClientSession();
        QueueFree();
    }
}