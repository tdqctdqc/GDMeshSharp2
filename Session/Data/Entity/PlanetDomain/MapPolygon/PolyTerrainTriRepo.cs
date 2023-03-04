
public class PolyTerrainTriRepo : Repository<PolyTerrainTris>
{
    public RepoIndexer<PolyTerrainTris, MapPolygon> ByPoly { get; private set; }
    public PolyTerrainTriRepo(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = new RepoIndexer<PolyTerrainTris, MapPolygon>(data,
            t => t.Poly.Entity());
    }
}
