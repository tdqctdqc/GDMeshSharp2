using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class StartScene : Node
{
    private ButtonToken _genBtn, _remoteBtn, _genPresetBtn, _sandbox;
    public override void _Ready()
    {
        _genBtn = ButtonToken.Get(this, "Generate", StartGenerator); 
        _remoteBtn = ButtonToken.Get(this, "Remote", StartAsClient); 
        _genPresetBtn = ButtonToken.Get(this, "GenPreset", StartAsTest); 
        _sandbox = ButtonToken.Get(this, "Sandbox", StartSandbox); 
    }

    private void StartAsTest()
    {
        Game.I.StartTestSession();
        QueueFree();
    }
    private void StartGenerator()
    {
        Game.I.StartGeneratorSession();
        QueueFree();
    }

    private void StartAsClient()
    {
        Game.I.StartClientSession();
        QueueFree();
    }

    private void StartSandbox()
    {
        Game.I.StartSandbox();
        QueueFree();
    }
}