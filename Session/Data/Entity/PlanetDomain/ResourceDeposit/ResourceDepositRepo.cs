
public class ResourceDepositRepo : EntityAux<ResourceDeposit>
{
    public EntityMultiIndexer<MapPolygon, ResourceDeposit> ByPoly { get; private set; }
    public ResourceDepositRepo(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = new EntityMultiIndexer<MapPolygon, ResourceDeposit>(data, r => r.Poly, nameof(ResourceDeposit.Poly));
    }
}
