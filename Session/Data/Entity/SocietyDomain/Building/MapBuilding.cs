
using System;
using MessagePack;

public class MapBuilding : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public PolyTriPosition Position { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }
    public float Efficiency { get; private set; } // out of 100

    public static MapBuilding Create(PolyTriPosition position, BuildingModel model, CreateWriteKey key)
    {
        var b = new MapBuilding(key.IdDispenser.GetID(), position, model.MakeRef(), 1f);
        key.Create(b);
        return b;
    }

    [SerializationConstructor] protected MapBuilding(int id, PolyTriPosition position, 
        ModelRef<BuildingModel> model, float efficiency) : base(id)
    {
        Position = position;
        Model = model;
        Efficiency = efficiency;
    }

    
}
