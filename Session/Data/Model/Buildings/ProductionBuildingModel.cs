
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ProductionBuildingModel : WorkBuildingModel
{
    public Item ProdItem { get; private set; }
    public abstract int ProductionCap { get; }
    public override int Capacity => ProductionCap;
    protected ProductionBuildingModel(Item prodItem, string name, int numTicksToBuild, int laborPerTickToBuild)
        : base(name, numTicksToBuild, laborPerTickToBuild)
    {
        ProdItem = prodItem;
    }

    public override void Produce(WorkProdConsumeProcedure proc, PolyTriPosition pos, float staffingRatio, 
        int ticksSinceLast, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var ratio = GetProductionRatio(pos, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * ProductionCap * ticksSinceLast);
        var rId = pos.Poly(data).Regime.RefId;
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
    }

    public abstract float GetProductionRatio(PolyTriPosition pos, float staffingRatio, Data data);
    public override float GetTriEfficiencyScore(PolyTriPosition pos, Data data) => GetProductionRatio(pos, 1f, data);
}
