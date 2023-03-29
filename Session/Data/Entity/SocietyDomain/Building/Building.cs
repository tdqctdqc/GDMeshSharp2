
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

    public static Building Create(PolyTriPosition position, BuildingModel model, CreateWriteKey key)
    {
        var b = new Building(key.IdDispenser.GetID(), position, model.MakeRef());
        key.Create(b);
        return b;
    }

    [SerializationConstructor] private Building(int id, PolyTriPosition position, ModelRef<BuildingModel> model) : base(id)
    {
        Position = position;
        Model = model;
    }

    
}
