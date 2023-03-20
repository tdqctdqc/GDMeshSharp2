
using Godot;

public class PlayerRegimeDisplay
{
    public static Node Create(Data data)
    {
        var label = new Label();
        var player = data.BaseDomain.Players.LocalPlayer;
        SubscribedStatLabel.ConstructForEntityDynamic(player, nameof(Player.Regime), label, p => p.Regime);
        return label;
    }
}
