
using System;
using System.Linq;
using Godot;

public class PolyIconsChunkGraphicFactory : ChunkGraphicFactory
{
    public PolyIconsChunkGraphicFactory(string name, bool active) 
        : base(name, active, (c, d) => new PolygonIconsChunkGraphic(
            c, d, 
            p => p.GetResourceDeposits(d)
                ?.Select(r => r.Resource.Model().ResIcon)))
    {
    }
}
