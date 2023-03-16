
using System;

public abstract class MapBuilding : Entity
{
    public PolyTriPosition Position { get; private set; }
    public ModelRef<BuildingModel> Model { get; private set; }
    protected MapBuilding(int id) : base(id)
    {
    }

    public override Type GetDomainType() => typeof(SocietyDomain);
}
