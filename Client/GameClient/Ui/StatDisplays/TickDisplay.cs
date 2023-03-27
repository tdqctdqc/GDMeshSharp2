
using Godot;

public class TickDisplay : Label
{
    private RefAction<ValChangeNotice<int>> _tick;

    public TickDisplay()
    {
        _tick = new RefAction<ValChangeNotice<int>>();
        _tick.Subscribe(n => Text = $"Tick: {n.NewVal}");
        Game.I.Client.Requests.SubscribeForValChange<GameClock, int>(
            nameof(GameClock.Tick),
            _tick
        );
    }
    public override void _ExitTree()
    {
        _tick.EndSubscriptions();
    }
}
