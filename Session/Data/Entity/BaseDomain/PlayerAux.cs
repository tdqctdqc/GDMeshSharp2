
    using System;
    using Godot;

    public class PlayerAux : EntityAux<Player>
    {
        public Entity1To1Indexer<Player, Regime> ByRegime { get; private set; }
        public Entity1to1PropIndexer<Player, Guid> ByGuid { get; private set; }
        public PlayerAux(Domain domain, Data data) : base(domain, data)
        {
            ByRegime = Entity1To1Indexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime, 
                nameof(Player.Regime));
            ByGuid = Entity1to1PropIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[Game.I.PlayerGuid];
        
    }
