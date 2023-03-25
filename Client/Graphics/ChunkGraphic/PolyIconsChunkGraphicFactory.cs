using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyIconsChunkGraphicFactory : ChunkGraphicFactory
{
    public PolyIconsChunkGraphicFactory(string name, bool active) : base(name, active) { }
    public override Node2D GetNode(MapChunk c, Data d)
    {
        return new PolygonIconsChunkGraphic(c, d, p => GetIconGroups(p, d), 1);
    }
    private IEnumerable<IEnumerable<Icon>> GetIconGroups(MapPolygon p, Data d)
    {
        return new List<IEnumerable<Icon>>
        {
            p.GetResourceDeposits(d)?.Select(r => r.Item.Model().ResIcon),
            p.GetPeeps(d)?.Select(peep => peep.Job.Model().JobIcon),
        };
    }
}
