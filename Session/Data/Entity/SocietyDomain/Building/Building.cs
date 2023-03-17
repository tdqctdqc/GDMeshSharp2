
using System;
using MessagePack;

public class Building : Entity
{
    public PolyTriPosition Position { get; private set; }
    public ModelRef<BuildingModel> Model { get; private set; }

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

    public override Type GetDomainType() => typeof(SocietyDomain);
}
