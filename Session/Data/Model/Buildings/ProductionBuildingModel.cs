
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ProductionBuildingModel : WorkBuildingModel
{
    public Item ProdItem { get; private set; }
    public abstract int ProductionCap { get; }
    protected ProductionBuildingModel(Item prodItem, string name, float buildCost)
        : base(name, buildCost)
    {
        ProdItem = prodItem;
    }

    public override void Produce(WorkProdConsumeProcedure proc, PolyTriPosition pos, float staffingRatio, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var ratio = GetProductionRatio(pos, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * ProductionCap);
        var rId = pos.Poly(data).Regime.RefId;
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
    }

    public abstract float GetProductionRatio(PolyTriPosition pos, float staffingRatio, Data data);
}
