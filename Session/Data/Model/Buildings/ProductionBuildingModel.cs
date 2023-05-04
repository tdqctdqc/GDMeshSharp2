
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

    public override void Produce(WorkProdConsumeProcedure proc, MapPolygon poly, float staffingRatio, 
        int ticksSinceLast, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var ratio = GetProductionRatio(poly, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * ProductionCap * ticksSinceLast);
        var rId = poly.Regime.RefId;
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
    }

    public abstract float GetProductionRatio(MapPolygon poly, float staffingRatio, Data data);
}
