
using System;

public abstract class ResourceProdBuilding : BuildingModel
{
    public Resource Resource { get; private set; }
    protected ResourceProdBuilding(Resource resource, string name) : base(name)
    {
        Resource = resource;
    }

    public ResourceProdResult Produce(MapPolygon p, float workerPower)
    {
        return new ResourceProdResult(Resource, p, ProdFunction(p, workerPower));
    }

    protected abstract float ProdFunction(MapPolygon p, float workerPower);
}
