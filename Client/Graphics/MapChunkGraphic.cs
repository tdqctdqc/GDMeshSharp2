using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunkGraphic : Node2D
{
    [Toggleable] public PolygonChunkGraphic Regimes { get; private set; }
    [Toggleable] public PolygonChunkGraphic Polys { get; private set; }
    [Toggleable] public TerrainTriChunkGraphic Landform { get; private set; }
    [Toggleable] public TerrainTriChunkGraphic Tris { get; private set; }
    [Toggleable] public TerrainTriChunkGraphic Vegetation { get; private set; }
    [Toggleable] public ChunkDecalGraphic Decals { get; private set; }
    [Toggleable] public RoadChunkGraphic Roads { get; private set; }
    [Toggleable] public BordersChunkGraphic Borders { get; private set; }

    public void Setup(MapChunk chunk, Data data)
    {
        var first = chunk.Polys.First();
        Position = first.Center;

        var polys = chunk.Polys.ToList();
        Polys = SetupPolygonGraphic(polys, data, 
            p => p.IsLand() ? Colors.SaddleBrown : Colors.Blue
        );
        Polys.Visible = false;
        
        Landform = SetupTerrainTriGraphic(polys, data, t => t.Landform.Color);

        Tris = SetupTerrainTriGraphic(polys, data, t => ColorsExt.GetRandomColor());
        Tris.Visible = false;

        Vegetation = new TerrainTriChunkGraphic();
        Vegetation.Setup(polys, data, pt => pt.Vegetation.Color.Darkened(pt.Landform.DarkenFactor));
        AddChild(Vegetation);

        Regimes = SetupPolygonGraphic(polys, data, 
            p => p.Regime.Empty()  
                ? Colors.Transparent
                : p.Regime.Entity().PrimaryColor
            );
        
        Roads = new RoadChunkGraphic();
        Roads.Setup(polys, data);
        AddChild(Roads);
        
        Decals = new ChunkDecalGraphic();
        Decals.Setup(chunk, first, data);
        AddChild(Decals);

        Borders = new BordersChunkGraphic();
        var borderCol = new Color(.75f, .75f, .75f, .5f);
        Borders.Setup(
            new List<List<LineSegment>>
            {
                chunk.Polys.SelectMany(p => p.BorderSegments.Select(bs => bs.Translate(first.GetOffsetTo(p, data)))).ToList(),
                chunk.Polys.SelectMany(p => p.GetTerrainTris(data).Tris.SelectMany(t => t.Transpose(first.GetOffsetTo(p, data)).GetSegments())).ToList()
            },
            new List<float>{5f, 1f},
            new List<Color>{borderCol, borderCol}
        );
        AddChild(Borders);
        Borders.Visible = false;
        Order(
            Tris, 
            Polys,
            Landform,
            Vegetation,
            Regimes,
            Borders,
            Decals,
            Roads
        );
    }

    private void Order(params Node2D[] nodes)
    {
        for (var i = 0; i < nodes.Length; i++)
        {
            nodes[i].ZIndex = i;
        }
    }
    private TerrainTriChunkGraphic SetupTerrainTriGraphic(List<MapPolygon> polys, Data data, 
        Func<PolyTri, Color> getColor)
    {
        var t = new TerrainTriChunkGraphic();
        t.Setup(polys, data, getColor);
        AddChild(t);
        t.ZAsRelative = false;
        return t;
    }

    private PolygonChunkGraphic SetupPolygonGraphic(List<MapPolygon> polys, Data data,
        Func<MapPolygon, Color> getColor)
    {
        var g = new PolygonChunkGraphic();
        g.Setup
        (polys, data, 
            getColor,
            false
        );
        AddChild(g);
        return g;
    }
    private PolygonChunkGraphic SetupPolygonWheelGraphic(List<MapPolygon> polys, Data data,
        Func<int, Color> getColor)
    {
        var g = new PolygonChunkGraphic();
        g.SetupWheel
        (polys, data, 
            getColor,
            false
        );
        AddChild(g);
        return g;
    }
}