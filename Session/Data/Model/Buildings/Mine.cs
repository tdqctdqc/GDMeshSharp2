using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : ExtractionBuildingModel
{
    public Mine(string name, Item prodItem) 
        : base(prodItem, name, true, 150, 300)
    {
        if (prodItem.Attributes.Has<MineableAttribute>() == false) throw new Exception();
    }

    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.MinerAttribute, 500}
        };

    public override int ProductionCap { get; } = 10;
    public override float GetProductionRatio(PolyTriPosition pos, float staffingRatio, Data data)
    {
        return staffingRatio;
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data) => CanBuildInTri(t);
    public static bool CanBuildInTri(PolyTri t)
    {
        return t.Landform.IsLand;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        var ds = p.GetResourceDeposits(data);
        return ds != null && ds.Any(d => d.Item.Model() == ProdItem);
    }

    public override float GetPolyEfficiencyScore(MapPolygon poly, Data data)
    {
        return poly.Roughness;
    }
}
