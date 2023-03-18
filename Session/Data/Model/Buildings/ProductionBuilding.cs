
using System;
using System.Collections.Generic;

public abstract class ProductionBuilding : BuildingModel
{
    public Item Item { get; private set; }
    public abstract int PeepsLaborReq { get; }
    public abstract HashSet<PeepJob> JobTypes { get; }
    protected ProductionBuilding(Item resource, string name) : base(name)
    {
        Item = resource;
    }

    public abstract void Produce(Wallet<ModelRef<Item>> gains, 
        Wallet<EntityRef<ResourceDeposit>> depletions, Building p, float staffingRatio, Data data);
}
