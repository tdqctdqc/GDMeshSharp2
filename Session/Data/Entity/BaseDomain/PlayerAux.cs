
    using System;
    using Godot;

    public class PlayerAux : EntityAux<Player>
    {
        public Entity1To1Indexer<Player, Regime> ByRegime { get; private set; }
        public Entity1to1PropIndexer<Player, Guid> ByGuid { get; private set; }
        public RefAction<ValChangeNotice<EntityRef<Regime>>> PlayerChangedRegime { get; private set; }
        public PlayerAux(Domain domain, Data data) : base(domain, data)
        {
            //todo fix
            var regimeVar = Game.I.Serializer.GetEntityMeta<Player>()
                .GetEntityVarMeta<EntityRef<Regime>>(nameof(Player.Regime));
            PlayerChangedRegime = regimeVar.ValChanged(); //todo make copy?
            ByRegime = Entity1To1Indexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime, PlayerChangedRegime);
            ByGuid = Entity1to1PropIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[Game.I.PlayerGuid];
    }
