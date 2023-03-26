
    using Godot;

    public class SettlementAux : EntityAux<Settlement>
    {
        public Entity1To1Indexer<Settlement, MapPolygon> ByPoly { get; private set; }
        public SettlementAux(Domain domain, Data data) : base(domain, data)
        {
            ByPoly = Entity1To1Indexer<Settlement, MapPolygon>
                .CreateStatic(data, s => s.Poly);
        }
    }
