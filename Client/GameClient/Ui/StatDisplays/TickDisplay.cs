
using Godot;

public class TickDisplay 
{
    public static Node Create()
    {
        var label = new Label();
        ValueChangedHandler<GameClock, int>.RegisterForAll(nameof(GameClock.Tick),
            n => label.Text = $"Tick: {n.NewVal}");
        return label;
    }
}
