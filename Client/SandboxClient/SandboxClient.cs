using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class SandboxClient : Node, IClient
{
    public CameraController Cam { get; private set; }
    private Vector2 _home;
    private Label _degrees, _mousePos;
    private Dictionary<int, Node2D> _triGraphics;
    private Node2D _mouseOverTriGraphics;
    private Line2D _mouseLine;
    private CanvasLayer _canvas;
    private Node2D _hook;
    private MockPolygon _poly;
    private int _prevMouseOver = -1;
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
        _hook = new Node2D();
        AddChild(_hook);
        this.AssignChildNode(ref _canvas, "Canvas");
        _home = home;
        var mb = new MeshBuilder();
        mb.AddPointMarkers(new List<Vector2>{_home}, 20f, Colors.Red);
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

    public void DrawPoly(MockPolygon poly)
    {
        _poly = poly;
        foreach (var keyValuePair in _triGraphics)
        {
            keyValuePair.Value.Free();
        }
        
        _triGraphics = new Dictionary<int, Node2D>();
        var mb = new MeshBuilder();
        var tris = poly.Tris.GetTris();
        
        for (var i = 0; i < tris.Count; i++)
        {
            var t = tris[i];
            mb.AddTriOutline(t, 5f, Colors.White);
            var tGraphic = mb.GetMeshInstance();
            AddChild(tGraphic);
            _triGraphics.Add(i, tGraphic);
            mb.Clear();
        }
    }
    public void Process(float delta)
    {
        var mousePos = _hook.GetGlobalMousePosition();
            
        var angle = (mousePos - _home).GetClockwiseAngle();
        _degrees.Text = "Degrees: " + ((int)angle.RadToDegrees()).ToString();
        _mousePos.Text = "Mouse Pos: " + mousePos.ToString();
        _mouseLine.ClearPoints();
        _mouseLine.AddPoint(_home);
        _mouseLine.AddPoint(mousePos);

        var sw = new Stopwatch();
        sw.Start();
        var intersecting = _poly.Tris.IntersectingTri(mousePos, out int section);
        sw.Stop();

        if (intersecting != -1 && intersecting != _prevMouseOver)
        {
            GD.Print("find time " + sw.Elapsed.TotalMilliseconds);
            _mouseOverTriGraphics?.Free();
            _prevMouseOver = intersecting;
            var mb = new MeshBuilder();
            
            mb.AddTri(_poly.Tris.GetTriangle(intersecting), Colors.Red);
            _mouseOverTriGraphics = mb.GetMeshInstance();
            AddChild(_mouseOverTriGraphics);
        }
        else if(intersecting != _prevMouseOver)
        {
            _mouseOverTriGraphics?.Free();
            _mouseOverTriGraphics = null;
        }
    }

}