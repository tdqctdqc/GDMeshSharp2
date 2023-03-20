
using System;
using System.Linq;
using Godot;

public class BorderChunkGraphicFactory : ChunkGraphicFactory
{
    public BorderChunkGraphicFactory(string name, bool active) 
        : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        var r = new BorderChunkGraphic();
        r.SetupForRegime(c.RelTo, c.Polys.ToList(),
            20f, d);
        return r;
    }
}
