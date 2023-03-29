
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BordersChunkGraphicFactory : ChunkGraphicFactory
{
    public BordersChunkGraphicFactory(string name, bool active) 
        : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        var b = new BordersChunkGraphic();
        var borderCol = new Color(.75f, .75f, .75f, .5f);
        b.Setup(
            new List<List<LineSegment>>
            {
                c.Polys.SelectMany(p =>
                    p.GetOrderedBoundarySegs(d).Select(bs => bs.Translate(c.RelTo.GetOffsetTo(p, d)))).ToList(),
                c.Polys.SelectMany(p =>
                        p.Tris.Tris.SelectMany(t => t.Transpose(c.RelTo.GetOffsetTo(p, d)).GetSegments()))
                    .ToList()
            },
            new List<float> {5f, 1f},
            new List<Color> {borderCol, borderCol}
        );
        return b;
    }
}
