
using System;
using Godot;

public class PolyTriChunkGraphicFactory : ChunkGraphicFactory
{
    public PolyTriChunkGraphicFactory(string name, bool active, Func<PolyTri, Color> getColor) 
        : base(name, active, 
            (c, d) =>
            {
                var t = new PolyTriChunkGraphic();
                t.Setup(c, d, getColor);
                t.ZAsRelative = false;
                return t;
            }
        )
    {
    }
}
