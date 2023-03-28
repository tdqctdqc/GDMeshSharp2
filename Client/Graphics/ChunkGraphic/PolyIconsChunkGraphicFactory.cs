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
        var rds = p.GetResourceDeposits(d);
        var peeps = p.GetPeeps(d);
        IEnumerable<Icon> peepGroup = null;
        if (peeps != null && peeps.Count() > 0)
            peepGroup = peeps.Where(pe => pe.Size > 0)
                .SelectMany(peep => Enumerable.Range(0, peep.Size).Select(i => peep.Job.Model().JobIcon));
        return new List<IEnumerable<Icon>>
        {
            rds?.Select(r => r.Item.Model().ResIcon),
            peepGroup
        };
    }
}
