
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

    public virtual void Produce(ItemWallet gains,
        EntityWallet<ResourceDeposit> depletions, Building p, float staffingRatio, Data data)
    {
        if (p.Model.Model() != this) throw new Exception();
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var ratio = GetProductionRatio(p, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * ProductionCap);
        gains.Add(ProdItem, prod);
    }

    public abstract float GetProductionRatio(Building p, float staffingRatio, Data data);
}
