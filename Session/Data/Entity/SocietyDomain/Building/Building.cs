
using System;
using MessagePack;

public class Building : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public PolyTriPosition Position { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }
    public float Efficiency { get; private set; } // out of 100

    public static Building Create(PolyTriPosition position, BuildingModel model, CreateWriteKey key)
    {
        var b = new Building(key.IdDispenser.GetID(), position, model.MakeRef(), 1f);
        key.Create(b);
        return b;
    }

    [SerializationConstructor] protected Building(int id, PolyTriPosition position, 
        ModelRef<BuildingModel> model, float efficiency) : base(id)
    {
        Position = position;
        Model = model;
        Efficiency = efficiency;
    }

    
}
