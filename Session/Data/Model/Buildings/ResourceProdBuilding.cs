
using System;

public abstract class ResourceProdBuilding : BuildingModel
{
    public Resource Resource { get; private set; }
    protected ResourceProdBuilding(Resource resource, string name) : base(name)
    {
        Resource = resource;
    }

    public abstract void Produce(ProductionResult result, Building p, float staffingRatio, Data data);
}
