
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : ProductionBuilding
{
    public Farm() 
        : base(ItemManager.Food, nameof(Farm), false, 100f)
    {
    }
    public override int PeepsLaborReq { get; } = 200;
    public override int FullProduction { get; } = 1000;
    public override HashSet<PeepJob> JobTypes { get; }
        = new HashSet<PeepJob> { PeepJobManager.Farmer };

    public override bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.Landform.IsLand
               && t.Landform.MinRoughness < LandformManager.Mountain.MinRoughness;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
    protected override float GetProductionRatio(Building p, float staffingRatio, Data data)
    {
         var tri = p.Position.Tri(data);
         return tri.Landform.FertilityMod * tri.Vegetation.FertilityMod * staffingRatio; 
    }
}
