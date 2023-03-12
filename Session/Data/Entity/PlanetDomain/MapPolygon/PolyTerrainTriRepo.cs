//
// public class PolyTerrainTriRepo : Repository<PolyTerrainTris>
// {
//     public RepoIndexer<PolyTerrainTris, MapPolygon> ByPoly { get; private set; }
//     public PolyTerrainTriRepo(Domain domain, Data data) : base(domain, data)
//     {
//         ByPoly = RepoIndexer<PolyTerrainTris, MapPolygon>.CreateStatic(data,
//             t => t.Poly.Entity());
//     }
// }
