using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GeneratorClient : Node
{
    private Node _node;
    private ButtonToken _generate, _generateNext, _generateTest, _done;
    private SpinBox _seed;
    private GeneratorGraphics _graphics;
    public GraphicLayerHolder Holder { get; private set; }
    public ButtonContainer Buttons { get; private set; }
    private WorldData _data;
    public static GeneratorClient Get()
        => (GeneratorClient) ((PackedScene) GD.Load("res://Client/GeneratorClient/GeneratorClient.tscn")).Instance();


    public void Done()
    {
        if (_data != null)
        {
            Game.I.StartHostSession(_data);
            QueueFree();
        }
    }
    public void Setup()
    {
        Holder = (GraphicLayerHolder)FindNode("GraphicLayerHolder");
        Holder.Setup();

        Buttons = (ButtonContainer) FindNode("ButtonContainer");
        _seed = (SpinBox) FindNode("Seed");
        
        _generate = ButtonToken.Get(this, "Generate", () => PressedGenerate());
        _generateNext = ButtonToken.Get(this, "GenerateNext", () => PressedGenerateNext());
        _generateTest = ButtonToken.Get(this, "GenerateTest", () => PressedGenerateTest());
        _done = ButtonToken.Get(this, "Done", Done);
        _graphics = (GeneratorGraphics) FindNode("Graphics");
        
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
        _data = worldGen.Data;
        worldGen.Generate();
        Holder.Clear();
        Buttons.Clear();
        _graphics.Setup(_data, this);
        
        AddPolyViewMode(_graphics.PolyGraphics, g => g.Cell.Plate.GetSeedPoly().Color, "Poly Plates");
        AddPolyViewMode(_graphics.PolyGraphics, g => g.Cell.Plate.Mass.GetSeedPoly().Color, "Poly Masses");
        AddPolyViewMode(_graphics.PolyGraphics, g => g.Color, "Polys");
        AddPolyViewMode(_graphics.PolyGraphics, g => g.Cell.Plate.Mass.Continent.GetSeedPoly().Color, "Poly Continents");
        AddPolyViewMode(_graphics.PolyGraphics, g => g.IsLand() ? Colors.SaddleBrown : Colors.Blue, "Land/Sea");
        AddPolyViewMode(_graphics.PolyGraphics, g => g.Cell.Seed.Color, "Poly Cells");
        AddPolyViewMode(_graphics.PolyGraphics, g => Colors.White.LinearInterpolate(Colors.Red, g.Roughness), "Poly Roughness");
        AddPolyViewMode(_graphics.PolyGraphics, g => Colors.White.LinearInterpolate(Colors.Blue, g.Moisture), "Poly Moisture");
        AddPolyViewMode(_graphics.PolyGraphics, g => _data.Landforms.GetAspectFromPoly(g, _data).Color, "Poly Landforms");

    }
    
    private void AddPolyViewMode(List<PolygonGraphic> polyGraphics, Func<GeoPolygon, Color> getColor, string name)
    {
        Buttons.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeoPolygon;
                var color = getColor(g);
                p.SetColor(color);
            });
        }, "Show " + name);
    }
}