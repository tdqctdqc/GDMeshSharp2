using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyIconsChunkGraphicFactory : ChunkGraphicFactory
{
    public PolyIconsChunkGraphicFactory(string name, bool active) : base(name, active) { }
    public override Node2D GetNode(MapChunk c, Data d)
    {
        return new PolygonIconsChunkGraphic(c, d, p => GetIconGroups(p, d));
    }
    private PolyIconGroups GetIconGroups(MapPolygon p, Data d)
    {


        var groups = new List<List<Icon>>();
        var labels = new List<List<string>>();
        var cutoffs = new List<float>();
        
        var peeps = p.GetPeeps(d)?.Where(pe => pe.Size > 0);
        if (peeps != null && peeps.Count() > 0)
        {
            groups.Add(peeps.Select(peep => peep.Job.Model().JobIcon).ToList());
            labels.Add(peeps.Select(peep => peep.Size.ToString()).ToList());
            cutoffs.Add(1.5f);
        }
        
        var rds = p.GetResourceDeposits(d);
        if (rds != null)
        {
            groups.Add(rds?.Select(r => r.Item.Model().ResIcon).ToList());
            labels.Add(rds.Select(rd => Mathf.CeilToInt(rd.Size).ToString()).ToList());
            cutoffs.Add(3f);
        }
        
        return new PolyIconGroups(groups, labels, cutoffs);
    }

    private List<Icon> GetPeepIconGroup(MapPolygon p, Data d)
    {
        
        return null;
    }
    
    private PolyIconGroups GetResourceIconGroup(MapPolygon p, Data d)
    {
        

        return null;
    }
}
