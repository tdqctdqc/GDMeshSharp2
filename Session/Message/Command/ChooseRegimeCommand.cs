
using System;
using Godot;

public class ChooseRegimeCommand : Command
{
    public EntityRef<Regime> Regime { get; private set; }
    public ChooseRegimeCommand(EntityRef<Regime> regime) : base()
    {
        Regime = regime;
    }

    public override void Enact(HostWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        player.Set(nameof(player.Regime), Regime, key);
    }

    public override bool Valid(Data data)
    {
        return Regime.Entity().IsPlayerRegime(data) == false;
    }
}
