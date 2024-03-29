using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MainMenuClient : Node, IClient
{
    public TooltipManager TooltipManager { get; }
    public ClientWriteKey Key { get; private set; }
    public ICameraController Cam { get; }
    public ClientSettings Settings { get; }
    public ClientRequests Requests { get; private set; }

    public MainMenuClient()
    {
        var startScene = SceneManager.Instance<StartScene>();
        AddChild(startScene);
        Key = new ClientWriteKey(null, null);
        Settings = ClientSettings.Load();
    }
    public void HandleInput(InputEvent e, float delta)
    {
    }

    public void Process(float delta, bool gameStateChanged)
    {
    }
    
}
