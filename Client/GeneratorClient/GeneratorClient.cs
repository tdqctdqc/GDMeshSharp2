using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class GeneratorClient : Node, IClient
{
    private GeneratorSession _session;
    private Node _node;
    private ButtonToken _generate, _generateNext, _generateTest, _done;
    private SpinBox _seed, _width, _height;
    public CameraController Cam { get; private set; }
    public GenData Data => _session.Data;
    private GeneratorGraphics _graphics;
    private Label _progress;
    private bool _generating;
    private bool _generated;
    private MapDisplayOptionsUi _mapOptions;

    public override void _Ready()
    {
        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;
        this.AssignChildNode(ref _seed, "Seed");
        this.AssignChildNode(ref _width, "Width");
        this.AssignChildNode(ref _height, "Height");
        this.AssignChildNode(ref _width, "Width");
        this.AssignChildNode(ref _progress, "Progress");
        this.AssignChildNode(ref _graphics, "Graphics");
        this.AssignChildNode(ref _mapOptions, "MapDisplayOptionsUi");
        _mapOptions.Setup(_graphics);
        
        _generate = ButtonToken.Get(this, "Generate", () => PressedGenerate());
        _done = ButtonToken.Get(this, "Done", GoToGameSession);
    }

    public void Setup(GeneratorSession session)
    {
        _session = session;
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }
    

    public void GoToGameSession()
    {
        if (_generated)
        {
            Game.I.StartHostSession(_session.Data);
            QueueFree();
        }
    }
    public void ProcessPoly(float delta)
    {
        if(_generating == false) _graphics?.Process(delta, _session.Data);
    }
    private async void PressedGenerate()
    {
        if (_generating) return;
        _generating = true;
        await Task.Run(() => Generate((int) _seed.Value, (int) _width.Value, (int) _height.Value)); 
        _generating = false;
        _generated = true;
    }

    private GenerationParameters GetParams()
    {
        return new GenerationParameters(new Vector2((int) _width.Value, (int) _height.Value));
    }

    private void MonitorGeneration(string tag, string progress)
    {
        _progress.Text = tag + " " + progress;
    }
    private void DisplayException(DisplayableException d)
    {
        AddChild(d.GetDisplay());
        GD.Print(d.StackTrace);
    }
    private void Generate(int seed, int width, int height)
    {
        var bounds = new Vector2(width, height);
        Game.I.Random.Seed = (ulong) seed;
        _node?.QueueFree();
        _node = new Node();
        AddChild(_node);
        MoveChild(_node, 0);
        _session.GenerationFeedback += MonitorGeneration;
        _session.GenerationFailed += DisplayException;
        var success = _session.Generate(seed, GetParams());
        if (success)
        {
            _graphics.Setup(this, _session.Data);
            _graphics.SetupGenerator(_session.Data, this);
        }
    }
}