
    public class PlayerRepo : Repository<Player>
    {
        public RepoEntityIndexer<Player, Regime> ByRegime { get; private set; }
        public PlayerRepo(Domain domain, Data data) : base(domain, data)
        {
            ByRegime = RepoEntityIndexer<Player, Regime>.CreateDynamic(data, p => p.Regime.Entity(), nameof(Player.Regime));
        }
    }
