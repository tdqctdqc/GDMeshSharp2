
using System;
using Godot;

public class PollingStatLabel : StatLabel
{
    private TimerAction _timer;
    
    public static PollingStatLabel ConstructForEntity<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat, float timerPeriod)
    {
        var d = new PollingStatLabel();
        d.Setup(e, name, label, getStat, timerPeriod);
        return d;
    }
    protected void Setup<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat, float timerPeriod)
    {
        _timer = new TimerAction(timerPeriod, timerPeriod, TriggerUpdate);
        base.Setup<TEntity, TProperty>(e, name, label, getStat);
    }

    public override void _Process(float delta)
    {
        _timer.Process(delta);
    }
}
