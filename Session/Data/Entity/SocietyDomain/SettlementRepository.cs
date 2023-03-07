
    using Godot;

    public class SettlementRepository : Repository<Settlement>
    {
        public RepoEntityIndexer<Settlement, MapPolygon> ByPoly { get; private set; }
        public SettlementRepository(Domain domain, Data data) : base(domain, data)
        {
            ByPoly = RepoEntityIndexer<Settlement, MapPolygon>
                .CreateStatic(data, s => s.Poly);
        }
    }
