
using System;
using System.Linq;
using Godot;

public class PolygonBorderChunkGraphicFactory : ChunkGraphicFactory
{
    public PolygonBorderChunkGraphicFactory(string name, bool active) 
        : base(name, active, (c, d) =>
        {
            var r = new PolygonBorderChunkGraphic();
            r.SetupForRegime(c.RelTo, c.Polys.ToList(),
                20f, d);
            return r;
        })
    {
    }
}
