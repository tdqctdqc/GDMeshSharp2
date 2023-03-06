
using System;
using Godot;

public class PollingStatDisplay : Node
{
    private TimerAction _timer;
    private Label _label;

    public static PollingStatDisplay Construct<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat, float timerPeriod)
    {
        
        var d = new PollingStatDisplay();
        d.Setup(e, name, label, getStat, timerPeriod);
        return d;
    }
    private void Setup<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat, float timerPeriod)
    {
        _label = label;
        _label.AddChild(this);
        _timer = new TimerAction(timerPeriod, timerPeriod,
            () => _label.Text = $"{name}: {getStat(e).ToString()}" );
    }

    public override void _Process(float delta)
    {
        _timer.Process(delta);
    }
}
