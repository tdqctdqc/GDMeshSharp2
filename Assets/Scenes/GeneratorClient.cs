using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class GeneratorClient : Node, IClient
{
    public ICameraController Cam { get; private set; }
    public ClientWriteKey Key { get; private set; }
    private MapGraphics _graphics;
    public CanvasLayer CanvasLayer => _ui;
    private GeneratorUi _ui; 
    public ClientSettings Settings { get; private set; }
    public ClientRequests Requests { get; private set; }
    public TooltipManager TooltipManager { get; private set; }

    public override void _Ready()
    {
        
    }

    public void Setup(GeneratorSession session)
    {
        Requests = new ClientRequests(session);
        Requests.GiveTree(session.Data.EntityTypeTree);
        Key = new ClientWriteKey(session.Data, session);
        Settings = ClientSettings.Load();
        var cam = CameraController.Construct(session.Data);
        AddChild(cam);
        cam.Current = true;
        Cam = cam;
        _graphics = new MapGraphics();
        AddChild(_graphics);
        TooltipManager = new TooltipManager(session.Data);
        AddChild(TooltipManager);
        _ui = GeneratorUi.Construct(this, session, _graphics);
        AddChild(_ui);
    }
    public void HandleInput(InputEvent e, float delta)
    {
    }
    public void Process(float delta)
    {
        _ui.Process(delta);
    }
}