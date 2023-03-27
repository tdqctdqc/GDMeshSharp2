
using Godot;

public class TickDisplay 
{
    public static Node Create()
    {
        var label = new Label();
        Game.I.Client.Requests.RegisterForAll<GameClock, int>(
            nameof(GameClock.Tick),
            n => label.Text = $"Tick: {n.NewVal}"
        );
        return label;
    }
}
