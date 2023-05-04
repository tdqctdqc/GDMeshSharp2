using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : ProductionBuildingModel
{
    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.ProleAttribute, 500}
        };
    public override int ProductionCap { get; } = 100;
    public Factory() : base(ItemManager.IndustrialPoint, nameof(Factory),
        100, 200)
    {
    }
    public override float GetProductionRatio(MapPolygon poly, float staffingRatio, Data data)
    {
        return staffingRatio;
    }

    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 1000}
        };
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand && p.HasSettlement(data);
    }

    public override float GetPolyEfficiencyScore(MapPolygon poly, Data data)
    {
        return 1f;
    }
}
