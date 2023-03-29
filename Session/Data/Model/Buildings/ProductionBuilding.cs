
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ProductionBuilding : BuildingModel
{
    public Item Item { get; private set; }
    public abstract int PeepsLaborReq { get; }
    public abstract HashSet<PeepJob> JobTypes { get; }
    public abstract int FullProduction { get; }
    public bool FromDeposit { get; private set; }
    protected ProductionBuilding(Item item, string name, bool fromDeposit, float buildCost)
        : base(name, buildCost)
    {
        Item = item;
        FromDeposit = fromDeposit;
    }

    public void Produce(ItemWallet gains,
        EntityWallet<ResourceDeposit> depletions, Building p, float staffingRatio, Data data)
    {
        if (p.Model.Model() != this) throw new Exception();
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var ratio = GetProductionRatio(p, staffingRatio, data);
        var prod = Mathf.FloorToInt(ratio * FullProduction);
        gains.Add(Item, prod);
        if (FromDeposit)
        {
            var deposit = p.Position.Poly(data).GetResourceDeposits(data)
                .First(d => d.Item.Model() == Item);
            depletions.Add(deposit, prod);
        }
    }

    protected abstract float GetProductionRatio(Building p, float staffingRatio, Data data);
}
