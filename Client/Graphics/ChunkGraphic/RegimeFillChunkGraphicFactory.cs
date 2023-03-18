
using Godot;

public static class RegimeFillChunkGraphicFactory
{
    public static ChunkGraphicFactory Create(string name, bool active)
    {
        return new ChunkGraphicFactory(name, false, 
            (c, d) => new PolygonChunkGraphic(
                c, d,
                p => p.Regime.Empty()
                    ? Colors.Transparent
                    : p.Regime.Entity().PrimaryColor
            )
        );
    }
}
