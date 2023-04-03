using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : ProductionBuildingModel
{
    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.LaborerAttribute, 500}
        };
    public override int ProductionCap { get; } = 100;
    public Factory() : base(ItemManager.IndustrialPoint, nameof(Factory),
        100f)
    {
    }
    public override float GetProductionRatio(Building p, float staffingRatio, Data data)
    {
        return staffingRatio;
    }
    
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand && p.HasSettlement(data);
    }
}
