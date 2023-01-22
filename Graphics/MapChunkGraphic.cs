using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunkGraphic : Node2D
{
    public PolygonChunkGraphic Regimes { get; private set; }
    public PolygonChunkGraphic Polys { get; private set; }
    public TerrainChunkGraphic Landform { get; private set; }
    public TerrainChunkGraphic Vegetation { get; private set; }
    public RoadChunkGraphic Roads { get; private set; }

    public void Setup(List<MapPolygon> chunk, Data data)
    {
        var first = chunk.First();
        Polys = new PolygonChunkGraphic();
        Polys.Setup(chunk, data, p => p.IsLand() 
        ? Colors.SaddleBrown 
        : Colors.Blue);
        AddChild(Polys);

        Landform = new TerrainChunkGraphic();
        Landform.Setup(chunk, data, data.Landforms);
        AddChild(Landform);
        
        Vegetation = new TerrainChunkGraphic();
        Vegetation.Setup(chunk, data, data.Vegetation);
        AddChild(Vegetation);
        
        Regimes = new PolygonChunkGraphic();
        Regimes.Setup(chunk, data, 
            p => p.Regime != null 
            ? new Color(p.Regime.Ref().PrimaryColor, .5f) 
            : Colors.Transparent);
        AddChild(Regimes);

        Roads = new RoadChunkGraphic();
        Roads.Setup(chunk, data);
        AddChild(Roads);
        
        Position = first.Center;

        Polys.ZAsRelative = false;
        Polys.ZIndex = 0;

        Landform.ZAsRelative = false;
        Landform.ZIndex = 1;

        Vegetation.ZAsRelative = false;
        Vegetation.ZIndex = 2;

        Roads.ZAsRelative = false;
        Roads.ZIndex = 3;

        Regimes.ZAsRelative = false;
        Regimes.ZIndex = 4;
    }

    private bool Toggle(Node2D n)
    {
        n.Visible = n.Visible == false;
        return n.Visible;
    }
    public bool ToggleRegimes()
    {
        return Toggle(Regimes);
    }
    public bool ToggleRoads()
    {
        return Toggle(Roads);
    }
    public bool ToggleLandforms()
    {
        return Toggle(Landform);
    }
    public bool ToggleVegetation()
    {
        return Toggle(Vegetation);
    }
}