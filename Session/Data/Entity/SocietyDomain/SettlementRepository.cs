
    using Godot;

    public class SettlementRepository : Repository<Settlement>
    {
        public Entity1To1EntityPropIndexer<Settlement, MapPolygon> ByPoly { get; private set; }
        public SettlementRepository(Domain domain, Data data) : base(domain, data)
        {
            ByPoly = Entity1To1EntityPropIndexer<Settlement, MapPolygon>
                .CreateStatic(data, s => s.Poly);
        }
    }
