
using System;
using Godot;

public class ChunkDecalGraphicFactory : ChunkGraphicFactory
{
    public ChunkDecalGraphicFactory(string name, bool active) 
        : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        var g = new ChunkDecalGraphic();
        g.Setup(c, d);
        return g;
    }
}
