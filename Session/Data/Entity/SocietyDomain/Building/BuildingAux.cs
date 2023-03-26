
public class BuildingAux : EntityAux<Building>
{
    public EntityMultiIndexer<MapPolygon, Building> ByPoly { get; private set; }
    public BuildingAux(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = new EntityMultiIndexer<MapPolygon, Building>(data, b => b.Position.Poly, nameof(Building.Position));
    }
}
