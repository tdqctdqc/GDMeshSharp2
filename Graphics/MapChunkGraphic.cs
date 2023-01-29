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

    public void Setup(MapChunk chunk, Data data)
    {
        var first = chunk.Polys.First();
        var polys = chunk.Polys.ToList();
        Polys = new PolygonChunkGraphic();
        Polys.Setup
        (chunk.Polys.ToList(), data, 
            p => p.IsLand() 
            ? Colors.SaddleBrown 
            : Colors.Blue
        );
        AddChild(Polys);

        Landform = new TerrainChunkGraphic();
        Landform.Setup(polys, data, data.Models.Landforms);
        AddChild(Landform);
        
        Vegetation = new TerrainChunkGraphic();
        Vegetation.Setup(polys, data, data.Models.Vegetation);
        AddChild(Vegetation);
        
        Regimes = new PolygonChunkGraphic();
        Regimes.SetupWithBorder(polys, 
            data, 
            p => p.Regime.Ref().PrimaryColor,
            (p1, p2) => p1.Regime.RefId == p2.Regime.RefId,
            p => p.Regime.Empty(),
            .25f, 1f, 25f);
        AddChild(Regimes);

        Roads = new RoadChunkGraphic();
        Roads.Setup(polys, data);
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