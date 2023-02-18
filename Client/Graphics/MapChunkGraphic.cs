using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunkGraphic : Node2D
{
    [Toggleable] public PolygonChunkGraphic Regimes { get; private set; }
    [Toggleable] public PolygonChunkGraphic Polys { get; private set; }
    [Toggleable] public PolygonChunkGraphic PolyWheelTris { get; private set; }
    [Toggleable] public TerrainTriChunkGraphic Landform { get; private set; }
    [Toggleable] public TerrainTriChunkGraphic Tris { get; private set; }
    [Toggleable] public VegetationChunkGraphic Vegetation { get; private set; }
    [Toggleable] public RoadChunkGraphic Roads { get; private set; }

    public void Setup(MapChunk chunk, Data data)
    {
        var first = chunk.Polys.First();
        var polys = chunk.Polys.ToList();
        Polys = SetupPolygonGraphic(polys, data, 
            p => p.IsLand() ? Colors.SaddleBrown : Colors.Blue, 
            0
        );
        Landform = SetupTerrainTriGraphic(polys, data, t => t.Landform.Color, 1);

        Tris = SetupTerrainTriGraphic(polys, data, t => ColorsExt.GetRandomColor(), 5);

        Vegetation = new VegetationChunkGraphic();
        Vegetation.Setup(polys, data);
        AddChild(Vegetation);
        Vegetation.ZAsRelative = false;
        Vegetation.ZIndex = 2;

        Regimes = SetupPolygonGraphic(polys, data, 
            p => p.Regime.Empty()  
                ? Colors.Transparent
                : p.Regime.Ref().PrimaryColor, 
            4);
        
        Roads = new RoadChunkGraphic();
        Roads.Setup(polys, data);
        AddChild(Roads);
        
        Position = first.Center;
        PolyWheelTris = SetupPolygonWheelGraphic(polys, data, i => Colors.Orange.GetPeriodicShade(i), 9);
        
        
        
        Roads.ZAsRelative = false;
        Roads.ZIndex = 3;
    }

    private TerrainTriChunkGraphic SetupTerrainTriGraphic(List<MapPolygon> polys, Data data, 
        Func<PolyTri, Color> getColor, int z)
    {
        var t = new TerrainTriChunkGraphic();
        t.Setup(polys, data, getColor);
        AddChild(t);
        t.ZAsRelative = false;
        t.ZIndex = z;
        return t;
    }

    private PolygonChunkGraphic SetupPolygonGraphic(List<MapPolygon> polys, Data data,
        Func<MapPolygon, Color> getColor, int z)
    {
        var g = new PolygonChunkGraphic();
        g.Setup
        (polys, data, 
            getColor,
            false
        );
        AddChild(g);
        g.ZAsRelative = false;
        g.ZIndex = z;
        return g;
    }
    private PolygonChunkGraphic SetupPolygonWheelGraphic(List<MapPolygon> polys, Data data,
        Func<int, Color> getColor, int z)
    {
        var g = new PolygonChunkGraphic();
        g.SetupWheel
        (polys, data, 
            getColor,
            false
        );
        AddChild(g);
        g.ZAsRelative = false;
        g.ZIndex = z;
        return g;
    }
}