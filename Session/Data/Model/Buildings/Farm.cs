
using System.Linq;
using Godot;

public class Farm : ResourceProdBuilding
{
    public Farm() : base(StratResourceManager.Food, nameof(Farm))
    {
    }

    public override bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.Landform.IsLand
               && t.Landform.MinRoughness < LandformManager.Mountain.MinRoughness;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand();
    }
    public override void Produce(ProductionResult result, Building p, float staffingRatio, Data data)
    {
        var fert = p.Position.Poly.Entity().GetFertility();
        var regime = p.Position.Poly.Entity().Regime.Entity();
        result.AddResult(regime, StratResourceManager.Food, fert * staffingRatio);
    }
}
