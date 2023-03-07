using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class SandboxClient : Node, IClient
{
    public CameraController Cam { get; private set; }
    private Label _degrees, _mousePos;
    private Dictionary<int, Node2D> _triGraphics;
    private Node2D _mouseOverTriGraphics;
    private Line2D _mouseLine;
    private CanvasLayer _canvas;
    private List<int> _sectionTriIndices;
    
    public SandboxClient()
    {
        
    }

    public override void _Ready()
    {
        this.AssignChildNode(ref _degrees, "Degrees");
        this.AssignChildNode(ref _mousePos, "MousePos");
    }

    public void Setup(Vector2 home)
    {
        
        this.AssignChildNode(ref _canvas, "Canvas");
        var mb = new MeshBuilder();
        mb.AddPointMarkers(new List<Vector2> {Vector2.Zero}, 20f, Colors.Red);
        AddChild(mb.GetMeshInstance());
        
        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;

        _triGraphics = new Dictionary<int, Node2D>();
        _mouseOverTriGraphics = new Node2D();
        _mouseLine = new Line2D();
        _mouseLine.DefaultColor = Colors.Red;
        _mouseLine.Width = 10;
        AddChild(_mouseLine);
    }

    public void HandleInput(InputEvent e, float delta)
    {
        if (e is InputEventMouseMotion m)
        {
            
            
        }
    }

    public void DrawTri(Triangle tri)
    {

        // if (_triGraphics.TryGetValue(-1, out var node))
        // {
        //     node.Free();
        //     _triGraphics.Remove(-1);
        // }
        var mb = new MeshBuilder();
        mb.AddTri(tri, ColorsExt.GetRandomColor());
        var mi = mb.GetMeshInstance();
        _triGraphics.Add(_triGraphics.Count, mi);
        AddChild(mi);
    }
    
    public void Process(float delta)
    {
        
    }

}