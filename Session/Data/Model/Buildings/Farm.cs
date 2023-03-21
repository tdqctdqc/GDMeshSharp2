
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : ProductionBuilding
{
    public Farm() : base(ItemManager.Food, nameof(Farm))
    {
    }
    public override int PeepsLaborReq { get; } = 10;
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


    public override void Produce(ItemWallet gains, 
        EntityWallet<ResourceDeposit> depletions, Building p, float staffingRatio, Data data)
    {
        var fert = p.Position.Poly.Entity().GetFertility();
        var regime = p.Position.Poly.Entity().Regime.Entity();
        gains.Add(ItemManager.Food, fert * staffingRatio / 1000f);
    }
}
