
using System;
using MessagePack;

public class Building : Entity
{
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(Building);
    public override Type GetDomainType() => typeof(SocietyDomain);
    public PolyTriPositionSerializable Position { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }

    public static Building Create(PolyTriPositionSerializable positionSerializable, BuildingModel model, CreateWriteKey key)
    {
        var b = new Building(key.IdDispenser.GetID(), positionSerializable, model.MakeRef());
        key.Create(b);
        return b;
    }

    [SerializationConstructor] private Building(int id, PolyTriPositionSerializable position, ModelRef<BuildingModel> model) : base(id)
    {
        Position = position;
        Model = model;
    }

    
}
