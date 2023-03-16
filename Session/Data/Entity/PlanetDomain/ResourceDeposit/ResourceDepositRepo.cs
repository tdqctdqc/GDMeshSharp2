
public class ResourceDepositRepo : Repository<ResourceDeposit>
{
    public RepoEntityMultiIndexer<MapPolygon, ResourceDeposit> ByPoly { get; private set; }
    public ResourceDepositRepo(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = new RepoEntityMultiIndexer<MapPolygon, ResourceDeposit>(data, r => r.Poly, nameof(ResourceDeposit.Poly));
    }
}
