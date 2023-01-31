using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GeneratorClient : Node, IClient
{
    private Node _node;
    private ButtonToken _generate, _generateNext, _generateTest, _done;
    private SpinBox _seed;
    private GeneratorGraphics _graphics;
    public void HandleInput(InputEvent e, float delta)
    {
        
    }
    
    
    
    public CameraController Cam { get; private set; }
    Data IClient.Data => Data;
    public GenData Data { get; private set; }
    public static GeneratorClient Get()
        => (GeneratorClient) ((PackedScene) GD.Load("res://Client/GeneratorClient/GeneratorClient.tscn")).Instance();

    private MapDisplayOptionsUi _mapOptions;

    public void Done()
    {
        if (Data != null)
        {
            Game.I.StartHostSession(Data);
            QueueFree();
        }
    }

    public void Process(float delta)
    {
        _graphics?.Process(delta, Data);
    }
    public void Setup()
    {
        Cam = new CameraController();
        AddChild(Cam);
        Cam.Current = true;
        _seed = (SpinBox) FindNode("Seed");
        
        _generate = ButtonToken.Get(this, "Generate", () => PressedGenerate());
        _generateNext = ButtonToken.Get(this, "GenerateNext", () => PressedGenerateNext());
        _generateTest = ButtonToken.Get(this, "GenerateTest", () => PressedGenerateTest());
        _done = ButtonToken.Get(this, "Done", Done);
        _graphics = (GeneratorGraphics) FindNode("Graphics");
        _mapOptions = (MapDisplayOptionsUi) FindNode("MapDisplayOptionsUi");
        _mapOptions.Setup(_graphics);

    }
    private void PressedGenerateTest()
    {
        for (int i = 0; i < 100; i++)
        {
            _seed.Value = _seed.Value + 1;
            Game.I.Random.Seed = (ulong) _seed.Value;
            Generate((int)_seed.Value);
        }
    }
    private void PressedGenerate()
    {
        Generate((int)_seed.Value);
    }
    private void PressedGenerateNext()
    {
        _seed.Value = _seed.Value + 1;
        Generate((int)_seed.Value);
    }
    private void Generate(int seed)
    {
        var bounds = new Vector2(16000, 8000);

        Game.I.Random.Seed = (ulong) seed;//DateTime.Now.Millisecond;
        _node?.QueueFree();
        _node = new Node();
        AddChild(_node);
        MoveChild(_node, 0);
        var worldGen = new WorldGenerator(bounds);
        Data = worldGen.Data;
        worldGen.Generate();
        _graphics.Setup(this, Data);
        _graphics.SetupGenerator(Data, this);
    }
}