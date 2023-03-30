
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
        return new PolyFillChunkGraphic(
            c, d,
            p => p.Regime.Empty()
                    ? Colors.Transparent
                    : p.Regime.Entity().PrimaryColor,
            1f, 
            p =>
            {
                p.Modulate = new Color(p.Modulate, Game.I.Client.Cam.ScaledZoomOut);
            }
        );
    }
}
