
using System;
using Godot;

public class RegimeFillChunkGraphicFactory : ChunkGraphicFactory
{
    public RegimeFillChunkGraphicFactory(string name, bool active) 
        : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        return new PolygonChunkGraphic(
            c, d,
            p => p.Regime.Empty()
                ? Colors.Transparent
                : new Color(p.Regime.Entity().PrimaryColor, .4f)
        );
    }
}
