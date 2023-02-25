
    public class SettlementRepository : Repository<Settlement>
    {
        public RepoAuxLookup<Settlement, MapPolygon> ByPoly { get; private set; }
        public SettlementRepository(Domain domain, Data data) : base(domain, data)
        {
            ByPoly = new RepoAuxLookup<Settlement, MapPolygon>(data, s => s.Poly.Ref(), out var update);
        }
    }
