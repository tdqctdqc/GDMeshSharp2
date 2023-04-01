
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : ProductionBuildingModel
{
    public Farm() 
        : base(ItemManager.Food, nameof(Farm), 100f)
    {
    }
    public override int PeepsLaborReq { get; } = 200;
    public override int ProductionCap { get; } = 1000;
    public override PeepJob JobType { get; }
        = PeepJobManager.Farmer;

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
