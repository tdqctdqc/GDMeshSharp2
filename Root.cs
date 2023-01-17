using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DelaunatorNetStd;

public class Root : Node
{
    public static RandomNumberGenerator Random = new RandomNumberGenerator();
    private Node _node;
    private Button _generate, _generateNext, _generateTest;
    private SpinBox _seed;
    private GraphicLayerHolder _holder;
    public static CameraController Cam { get; private set; }
    public static ButtonContainer ButtonContainer { get; private set; }
    public static Vector2 Bounds { get; private set; }
    public static WorldData WorldData { get; private set; }
    public override void _Ready()
    {
        Cam = (CameraController) FindNode("Camera2D");
        ButtonContainer = (ButtonContainer) FindNode("ButtonContainer");
        _holder = (GraphicLayerHolder)FindNode("GraphicLayerHolder");
        _holder.Setup();
        _seed = (SpinBox) FindNode("Seed");
        _generate = (Button) FindNode("Generate");
        _generate.Connect("button_up", this, nameof(PressedGenerate));
        _generateNext = (Button) FindNode("GenerateNext");
        _generateNext.Connect("button_up", this, nameof(PressedGenerateNext));
        _generateTest = (Button) FindNode("GenerateTest");
        _generateTest.Connect("button_up", this, nameof(PressedGenerateTest));
    }

    private void PressedGenerateTest()
    {
        for (int i = 0; i < 100; i++)
        {
            _seed.Value = _seed.Value + 1;
            Random.Seed = (ulong) _seed.Value;
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
        Bounds = new Vector2(16000, 8000);

        _holder.Clear();
        ButtonContainer.Clear();
        Random.Seed = (ulong) seed;//DateTime.Now.Millisecond;
        _node?.QueueFree();
        _node = new Node();
        AddChild(_node);
        MoveChild(_node, 0);
        var worldGen = new WorldGenerator(Bounds);
        WorldData = worldGen.Data;
        var sw = new Stopwatch();
        sw.Start();
        worldGen.Generate();
        sw.Stop();
        GD.Print("World gen time " + sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
        Cam.SetBounds(WorldData.Dimensions);
        Bounds = WorldData.Dimensions;
        
        sw.Start();
        Graphics.BuildGraphics(_node, _holder, WorldData);
        sw.Stop();
        GD.Print("Graphics gen time " + sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
    }

}
