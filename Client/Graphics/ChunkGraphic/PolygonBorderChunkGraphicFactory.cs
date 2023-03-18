
using System;
using System.Linq;
using Godot;

public class PolygonBorderChunkGraphicFactory : ChunkGraphicFactory
{
    public PolygonBorderChunkGraphicFactory(string name, bool active) 
        : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        var r = new PolygonBorderChunkGraphic();
        r.SetupForRegime(c.RelTo, c.Polys.ToList(),
            20f, d);
        return r;
    }
}
