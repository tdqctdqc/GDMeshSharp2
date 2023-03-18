
using System;
using System.Linq;
using Godot;

public class PolygonChunkGraphicFactory : ChunkGraphicFactory
{
    public PolygonChunkGraphicFactory(string name, bool active, Func<MapPolygon, Data, Color> getColor) 
        : base(name, active, (c, d) => new PolygonChunkGraphic(c, d, p => getColor(p,d)))
    {
    }
}
