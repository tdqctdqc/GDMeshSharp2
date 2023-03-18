
using System;

public abstract class ResourceProdBuilding : BuildingModel
{
    public StratResource StratResource { get; private set; }
    protected ResourceProdBuilding(StratResource resource, string name) : base(name)
    {
        StratResource = resource;
    }

    public abstract void Produce(ProductionResult result, Building p, float staffingRatio, Data data);
}
