
    public class SettlementRepository : Repository<Settlement>
    {
        public RepoEntityIndexer<Settlement, MapPolygon> ByPoly { get; private set; }
        public SettlementRepository(Domain domain, Data data) : base(domain, data)
        {
            ByPoly = new RepoEntityIndexer<Settlement, MapPolygon>(data, s => s.Poly.Entity());
        }
    }
