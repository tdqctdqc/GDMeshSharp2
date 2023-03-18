
using System;
using Godot;

public class ChunkDecalGraphicFactory : ChunkGraphicFactory
{
    public ChunkDecalGraphicFactory(string name, bool active) 
        : base(name, active, (c, d) =>
        {
            var g = new ChunkDecalGraphic();
            g.Setup(c, d);
            return g;
        })
    {
    }
}
