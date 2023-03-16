
using System;

public class ResourceDeposit : Entity
{
    public ModelRef<Resource> Resource { get; private set; }
    public float Size { get; private set; }
    public EntityRef<MapPolygon> Poly { get; private set; }
    public static ResourceDeposit Create(Resource resource, MapPolygon poly, float size, CreateWriteKey key)
    {
        var d = new ResourceDeposit(key.IdDispenser.GetID(), resource.MakeRef(), poly.MakeRef(), size);
        key.Create(d);
        return d;
    }

    private ResourceDeposit(int id, ModelRef<Resource> resource, EntityRef<MapPolygon> poly, float size) : base(id)
    {
        Resource = resource;
        Size = size;
        Poly = poly;
    }

    public override Type GetDomainType() => typeof(PlanetDomain);
}
