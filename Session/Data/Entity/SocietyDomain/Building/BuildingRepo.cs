
public class BuildingRepo : Repository<Building>
{
    public RepoEntityMultiIndexer<MapPolygon, Building> ByPoly { get; private set; }
    public BuildingRepo(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = new RepoEntityMultiIndexer<MapPolygon, Building>(data, b => b.Position.Poly, nameof(Building.Position));
    }
}
