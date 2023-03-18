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
            Roads,
            PolyIcons,
            ResourceDepositPolyFill,
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
    public static ChunkGraphicFactory Buildings { get; private set; }
        = new BuildingsChunkGraphicFactory(nameof(Buildings), true);
    public static ChunkGraphicFactory RegimeFill { get; private set; }
        = new RegimeFillChunkGraphicFactory(nameof(RegimeFill), false);
    public static ChunkGraphicFactory RegimeBorders { get; private set; }
        = new PolygonBorderChunkGraphicFactory(nameof(RegimeBorders), true);
    public static ChunkGraphicFactory Landform { get; private set; }
        = new PolyTriChunkGraphicFactory(nameof(Landform), true, t => t.Landform.Color);
    public static ChunkGraphicFactory Vegetation { get; private set; }
        = new PolyTriChunkGraphicFactory(nameof(Vegetation),true, 
            pt => pt.Vegetation.Color.Darkened(pt.Landform.DarkenFactor));
    public static ChunkGraphicFactory Tris { get; private set; }
        = new PolyTriChunkGraphicFactory(nameof(Tris), false, t => ColorsExt.GetRandomColor());
    public static ChunkGraphicFactory Decals { get; private set; }
        = new ChunkDecalGraphicFactory(nameof(Decals), false);
    public static ChunkGraphicFactory Roads { get; private set; }
        = new RoadChunkGraphicFactory(nameof(Roads), true);
    public static ChunkGraphicFactory Borders { get; private set; }
        = new BordersChunkGraphicFactory(nameof(Borders), false);
    public static ChunkGraphicFactory PolyIcons { get; private set; }
        = new PolyIconsChunkGraphicFactory(nameof(PolyIcons), true);
    public static ChunkGraphicFactory ResourceDepositPolyFill { get; private set; }
        = new PolygonChunkGraphicFactory(nameof(ResourceDepositPolyFill), true, (p,d) =>
            {
                var rs = p.GetResourceDeposits(d);
                if(rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                return rs.First().Resource.Model().Color;
            }
        );
}