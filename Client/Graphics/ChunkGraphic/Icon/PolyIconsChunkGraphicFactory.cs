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
    private IconGroups GetIconGroups(MapPolygon p, Data d)
    {
        var groups = new List<IIconGroupController>();
        
        var peeps = p.GetPeeps(d);
        if (peeps != null)
        {
            var pCon = new IconGroupController<KeyValuePair<PeepJob, int>>(
                peeps.SelectMany(peep => peep.Jobs)
                    .Where(kvp => kvp.Value.Count > 0)
                    .SortInto(ja => ja.Value.Job.Model(), 
                        ja => ja.Value.Count).ToList(),
                kvp => kvp.Value.ToString(),
                kvp => kvp.Key.JobIcon,
                1.5f
            );
            groups.Add(pCon);
        }
        
        
        var rds = p.GetResourceDeposits(d);
        if (rds != null)
        {
            var rdCon = new IconGroupController<ResourceDeposit>(
                rds.ToList(),
                rd => Mathf.CeilToInt(rd.Size).ToString(),
                rd => rd.Item.Model().Icon,
                4f
            );
            groups.Add(rdCon);
        }
        
        return new IconGroups(groups);
    }

    private List<Icon> GetPeepIconGroup(MapPolygon p, Data d)
    {
        
        return null;
    }
    
    private IconGroups GetResourceIconGroup(MapPolygon p, Data d)
    {
        

        return null;
    }
}
