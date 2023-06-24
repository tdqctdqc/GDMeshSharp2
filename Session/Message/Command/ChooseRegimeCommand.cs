
using System;
using Godot;

public class ChooseRegimeCommand : Command
{
    public EntityRef<Regime> Regime { get; private set; }
    public ChooseRegimeCommand(EntityRef<Regime> regime) : base()
    {
        Regime = regime;
    }

    public override void Enact(HostWriteKey key, Action<Procedure> queueProcedure)
    {
        GD.Print("enacting choose regime command");
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        player.Set<EntityRef<Regime>>(nameof(player.Regime), Regime, key);
        GD.Print(Regime.Entity().IsPlayerRegime(key.Data));
    }

    public override bool Valid(Data data)
    {
        return Regime.Entity().IsPlayerRegime(data) == false;
    }
}
