
    using System;
    using Godot;

    public class PlayerRepo : Repository<Player>
    {
        public RepoEntityIndexer<Player, Regime> ByRegime { get; private set; }
        public RepoIndexer<Player, Guid> ByGuid { get; private set; }
        public PlayerRepo(Domain domain, Data data) : base(domain, data)
        {
            ByRegime = RepoEntityIndexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime.Entity(), 
                nameof(Player.Regime));
            ByGuid = RepoIndexer<Player, Guid>.CreateStatic(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[Game.I.PlayerGuid];
        
    }
