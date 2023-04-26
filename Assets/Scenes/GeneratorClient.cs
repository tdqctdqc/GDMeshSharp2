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
    private Node2D _camTest;
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
        // _camTest?.QueueFree();
        // var mb = new MeshBuilder();
        // var screenSize = OS.WindowSize;
        // var leftX = -screenSize.x / 2f;
        // var rightX = screenSize.x / 2f;
        // var topY = -screenSize.y / 2f;
        // var bottomY = screenSize.y / 2f;
        // var tl = new Vector2(leftX, topY);
        // var tr = new Vector2(rightX, topY);
        // var bl = new Vector2(leftX, bottomY);
        // var br = new Vector2(rightX, bottomY);
        //
        // var zoom = Cam.ZoomOut;
        // mb.AddLine(Cam.Position + tl * zoom, Cam.Position + tr * zoom, Colors.Red, 100f);
        // mb.AddLine(Cam.Position + br * zoom, Cam.Position + tr * zoom, Colors.Red, 100f);
        // mb.AddLine(Cam.Position + tl * zoom, Cam.Position + bl * zoom, Colors.Red, 100f);
        // mb.AddLine(Cam.Position + br * zoom, Cam.Position + bl * zoom, Colors.Red, 100f);
        //
        // _camTest = mb.GetMeshInstance();
        // _camTest.ZIndex = 99;
        // _camTest.ZAsRelative = false;
        // AddChild(_camTest);
        //
        //
        //
        // var globalPos = Cam.GetGlobalMousePosition();
        // GD.Print(Cam.InScreen(globalPos));
    }
}