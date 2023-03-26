
using System;
using MessagePack;

public class ResourceDeposit : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public ModelRef<Item> Item { get; protected set; }
    public float Size { get; protected set; }
    public EntityRef<MapPolygon> Poly { get; protected set; }
    public static ResourceDeposit Create(Item resource, MapPolygon poly, float size, CreateWriteKey key)
    {
        var d = new ResourceDeposit(key.IdDispenser.GetID(), resource.MakeRef(), poly.MakeRef(), size);
        key.Create(d);
        return d;
    }

    [SerializationConstructor] private ResourceDeposit(int id, ModelRef<Item> item, EntityRef<MapPolygon> poly, float size) : base(id)
    {
        Item = item;
        Size = size;
        Poly = poly;
    }

    
}
