
using System;
using MessagePack;

public class Building : Entity
{
    public override Type GetRepoEntityType() => GetType();
    public PolyTriPositionSerializable Position { get; private set; }
    public ModelRef<BuildingModel> Model { get; private set; }

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

    public override Type GetDomainType() => typeof(SocietyDomain);
}
