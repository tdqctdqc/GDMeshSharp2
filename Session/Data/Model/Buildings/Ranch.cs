using System;
using System.Collections.Generic;
using System.Linq;

public class Ranch : ProductionBuildingModel
{
    public Ranch() 
        : base(BuildingType.Grazing, ItemManager.Food, nameof(Ranch), 
            25, 
            100
        )
    {
    }
    public override int ProductionCap { get; } = 1000;
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Food, 1}
        };
    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.FarmerAttribute, 100}
        };
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand
               && t.Landform.MinRoughness < LandformManager.Mountain.MinRoughness;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
