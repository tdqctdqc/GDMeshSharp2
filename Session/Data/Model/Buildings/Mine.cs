using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : ProductionBuilding
{
    public Mine(string name, Item item) : base(item, name, true)
    {
        if (item.Attributes.Has<MineableAttribute>() == false) throw new Exception();
    }
    public override int PeepsLaborReq { get; } = 5;
    public override HashSet<PeepJob> JobTypes { get; }
        = new HashSet<PeepJob>
        {
            PeepJobManager.Laborer
        };

    public override int FullProduction { get; } = 10;
    protected override float GetProductionRatio(Building p, float staffingRatio, Data data)
    {
        return staffingRatio;
    }

    public override bool CanBuildInTri(PolyTri t, Data data) => CanBuildInTri(t);

    public static bool CanBuildInTri(PolyTri t)
    {
        return t.Landform.IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        var ds = p.GetResourceDeposits(data);
        return ds != null && ds.Any(d => d.Item.Model() == Item);
    }
}
