using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;

public class Root : Node
{
    public static RandomNumberGenerator Random = new RandomNumberGenerator();
    private Node _node;
    private Button _generate, _generateNext, _generateTest;
    private SpinBox _seed;
    private GraphicLayerHolder _holder;
    private CameraController _cam;
    public override void _Ready()
    {
        _cam = (CameraController) FindNode("Camera2D");
        _holder = (GraphicLayerHolder)FindNode("GraphicLayerHolder");
        _holder.Setup();
        _seed = (SpinBox) FindNode("Seed");
        _generate = (Button) FindNode("Generate");
        _generate.Connect("button_up", this, nameof(PressedGenerate));
        _generateNext = (Button) FindNode("GenerateNext");
        _generateNext.Connect("button_up", this, nameof(PressedGenerateNext));
        _generateTest = (Button) FindNode("GenerateTest");
        _generateTest.Connect("button_up", this, nameof(PressedGenerateTest));
        
        
        GD.Print(Vector2.Right.AngleTo(Vector2.Down).RadToDegrees());
        GD.Print(Vector2.Right.AngleTo(Vector2.Left).RadToDegrees());
        GD.Print(Vector2.Right.AngleTo(Vector2.Up).RadToDegrees());
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

        _holder.Clear();
        Random.Seed = (ulong) seed;//DateTime.Now.Millisecond;
        _node?.QueueFree();
        _node = new Node();
        AddChild(_node);
        MoveChild(_node, 0);

        var worldData = WorldGenerator.Generate();
        Graphics.BuildGraphics(_node, _holder, worldData);
    }

}
