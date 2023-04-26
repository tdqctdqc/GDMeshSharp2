using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunkGraphicHolder : Node2D
{
    public Dictionary<string, MapChunkGraphicModule> Modules { get; private set; }
    private MapChunk _chunk;
    private Data _data;
    public void Setup(MapGraphics mg, MapChunk chunk, Data data)
    {
        _chunk = chunk;
        _data = data;
        Position = chunk.RelTo.Center;
        Modules = new Dictionary<string, MapChunkGraphicModule>();
        Order(
            chunk, data, mg,
            Tris, 
            Landform,
            Vegetation,
            RegimeFill,
            Roads,
            ResourceDepositPolyFill,
            RegimeBorders,
            Icons
        );
    }

    public void Update()
    {
        foreach (var m in Modules.Values)
        {
            m.Update(_data);
        }
    }
    private void Order(MapChunk chunk, Data data, MapGraphics mg, params ChunkGraphicFactory[] factories)
    {
        for (var i = 0; i < factories.Length; i++)
        {
            if (factories[i].Active == false) continue;
            var node = factories[i].GetModule(chunk, data, mg);
            node.ZIndex = i;
            Modules.Add(factories[i].Name, node);
            AddChild(node);
        }
    }
    public static ChunkGraphicFactory RegimeBorders { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(RegimeBorders), true,
            (c, d, mg) => BorderChunkGraphic.ConstructRegimeBorder(c, mg, 20f, d));
    public static ChunkGraphicFactory Landform { get; private set; }
        = new PolyTriChunkGraphicFactory(nameof(Landform), true, t => t.Landform.Color);
    public static ChunkGraphicFactory Vegetation { get; private set; }
        = new PolyTriChunkGraphicFactory(nameof(Vegetation),true, 
            pt => pt.Vegetation.Color.Darkened(pt.Landform.DarkenFactor));
    public static ChunkGraphicFactory Tris { get; private set; }
        = new PolyTriChunkGraphicFactory(nameof(Tris), 
            false, t => ColorsExt.GetRandomColor());
    public static ChunkGraphicFactory Roads { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(Roads), true,
            (c, d, mg) => new RoadChunkGraphicModule(c, d, mg));
    public static ChunkGraphicFactory Icons { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(Icons), true,
            (c, d, mg) => new IconsMapChunkGraphicModule(c, d, mg));
    public static ChunkGraphicFactory ResourceDepositPolyFill { get; private set; }
        = new PolygonFillChunkGraphicFactory(nameof(ResourceDepositPolyFill), false, (p,d) =>
            {
                var rs = p.GetResourceDeposits(d);
                if(rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                return rs.First().Item.Model().Color;
            }
        );
    public static ChunkGraphicFactory RegimeFill { get; private set; }
        = new PolygonFillChunkGraphicFactory(nameof(RegimeFill), true, (p,d) =>
            {
                if (p.Regime.Fulfilled()) return p.Regime.Entity().PrimaryColor;
                return Colors.Transparent;
            }
        );
}