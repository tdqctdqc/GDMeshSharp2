
using System;
using System.Linq;
using Godot;

public class PolygonChunkGraphicFactory : ChunkGraphicFactory
{
    private Func<MapPolygon, Data, Color> _getColor;
    public PolygonChunkGraphicFactory(string name, bool active, Func<MapPolygon, Data, Color> getColor) 
        : base(name, active)
    {
        _getColor = getColor;
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        return new PolyFillChunkGraphic(c, d, p => _getColor(p, d));
    }
}
