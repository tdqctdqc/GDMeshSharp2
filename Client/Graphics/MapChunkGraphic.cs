using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunkGraphic : Node2D
{
    public Dictionary<string, Node2D> Modules { get; private set; }
    

    public void Setup(MapChunk chunk, Data data)
    {
        Position = chunk.RelTo.Center;
        Modules = new Dictionary<string, Node2D>();
        Order(
            chunk, data,
            Tris, 
            Landform,
            Vegetation,
            RegimeFill,
            RegimeBorders,
            Borders,
            Decals,
            ResourceDeposits,
            Roads,
            Buildings
        );
    }

    private void Order(MapChunk chunk, Data data, params ChunkGraphicFactory[] factories)
    {
        for (var i = 0; i < factories.Length; i++)
        {
            if (factories[i].Active == false) continue;
            var node = factories[i].GetNode(chunk, data);
            node.ZAsRelative = false;
            node.ZIndex = i;
            Modules.Add(factories[i].Name, node);
            AddChild(node);
        }
    }
    private static TerrainTriChunkGraphic SetupTerrainTriGraphic(MapChunk chunk, Data data, 
        Func<PolyTri, Color> getColor)
    {
        var t = new TerrainTriChunkGraphic();
        t.Setup(chunk, data, getColor);
        t.ZAsRelative = false;
        return t;
    }

    private static PolygonChunkGraphic SetupPolygonGraphic(MapChunk chunk, Data data,
        Func<MapPolygon, Color> getColor)
    {
        var g = new PolygonChunkGraphic();
        g.Setup
        (chunk, data, 
            getColor,
            false
        );
        return g;
    }
    public static ChunkGraphicFactory Buildings { get; private set; }
        = new ChunkGraphicFactory(nameof(Buildings), true, 
            (c, d) => new BuildingChunkGraphic(c, d)
        );
    public static ChunkGraphicFactory RegimeFill { get; private set; }
        = new ChunkGraphicFactory(nameof(RegimeFill), false, 
            (c, d) => SetupPolygonGraphic(
                c, d,
                p => p.Regime.Empty()
                    ? Colors.Transparent
                    : p.Regime.Entity().PrimaryColor
            )
        );

    public static ChunkGraphicFactory RegimeBorders { get; private set; }
        = new ChunkGraphicFactory(nameof(RegimeBorders), true, (c, d) =>
        {
            var r = new PolygonBorderChunkGraphic();
            r.SetupForRegime(c.RelTo, c.Polys.ToList(),
                20f, d);
            return r;
        });

    public static ChunkGraphicFactory Landform { get; private set; }
        = new ChunkGraphicFactory(nameof(Landform), true, (c, d) =>
        {
            return SetupTerrainTriGraphic(c, d, t => t.Landform.Color);
        });
    
    public static ChunkGraphicFactory Vegetation { get; private set; }
        = new ChunkGraphicFactory(nameof(Vegetation),true, (c, d) =>
        {
            var v = new TerrainTriChunkGraphic();
            v.Setup(c, d, pt => pt.Vegetation.Color.Darkened(pt.Landform.DarkenFactor));
            return v;
        });
    
    public static ChunkGraphicFactory Tris { get; private set; }
        = new ChunkGraphicFactory(nameof(Tris), false, (c, d) =>
        {
            return SetupTerrainTriGraphic(c, d, t => ColorsExt.GetRandomColor());
        });
    public static ChunkGraphicFactory Decals { get; private set; }
        = new ChunkGraphicFactory(nameof(Decals), false, (c, d) =>
        {
            var g = new ChunkDecalGraphic();
            g.Setup(c, d);
            return g;
        });
    public static ChunkGraphicFactory Roads { get; private set; }
        = new ChunkGraphicFactory(nameof(Roads), true, (c, d) =>
        {
            var r = new RoadChunkGraphic();
            r.Setup(c, d);
            return r;
        });
    public static ChunkGraphicFactory Borders { get; private set; }
        = new ChunkGraphicFactory(nameof(Borders), false, (c, d) =>
        {
            var b = new BordersChunkGraphic();
            var borderCol = new Color(.75f, .75f, .75f, .5f);
            b.Setup(
                new List<List<LineSegment>>
                {
                    c.Polys.SelectMany(p => p.GetOrderedBoundarySegs(d).Select(bs => bs.Translate(c.RelTo.GetOffsetTo(p, d)))).ToList(),
                    c.Polys.SelectMany(p => p.TerrainTris.Tris.SelectMany(t => t.Transpose(c.RelTo.GetOffsetTo(p, d)).GetSegments())).ToList()
                },
                new List<float>{5f, 1f},
                new List<Color>{borderCol, borderCol}
            );
            return b;
        });
    public static ChunkGraphicFactory ResourceDeposits { get; private set; }
        = new ChunkGraphicFactory(nameof(ResourceDeposits), true, 
            (c, d) => SetupPolygonGraphic(
                c, d,
                p =>
                {
                    var rs = p.GetResourceDeposits(d);
                    if(rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                    return rs.First().Resource.Model().Color;
                }
            )
        );
}