
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : ProductionBuildingModel
{
    public Farm() 
        : base(ItemManager.Food, nameof(Farm), 100f)
    {
    }
    public override int ProductionCap { get; } = 1000;
    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.FarmerAttribute, 500}
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
    public override float GetProductionRatio(Building p, float staffingRatio, Data data)
    {
         var tri = p.Position.Tri(data);
         return tri.Landform.FertilityMod * tri.Vegetation.FertilityMod * staffingRatio; 
    }
}
