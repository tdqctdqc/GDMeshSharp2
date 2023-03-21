
    using System;
    using Godot;

    public class PlayerRepo : Repository<Player>
    {
        public Entity1To1EntityPropIndexer<Player, Regime> ByRegime { get; private set; }
        public Entity1to1PropIndexer<Player, Guid> ByGuid { get; private set; }
        public PlayerRepo(Domain domain, Data data) : base(domain, data)
        {
            ByRegime = Entity1To1EntityPropIndexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime, 
                nameof(Player.Regime));
            ByGuid = Entity1to1PropIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[Game.I.PlayerGuid];
        
    }
