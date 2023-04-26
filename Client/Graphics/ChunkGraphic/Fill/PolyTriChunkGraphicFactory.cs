
using System;
using Godot;

public class PolyTriChunkGraphicFactory : ChunkGraphicFactory
{
    private Func<PolyTri, Color> _getColor;
    public PolyTriChunkGraphicFactory(string name, bool active, Func<PolyTri, Color> getColor) 
        : base(name, active)
    {
        _getColor = getColor;
    }

    public override MapChunkGraphicModule GetModule(MapChunk c, Data d, MapGraphics mg)
    {
        var t = new PolyTriChunkGraphic();
        t.Setup(c, d, _getColor);
        t.ZAsRelative = false;
        return t;
    }
}
