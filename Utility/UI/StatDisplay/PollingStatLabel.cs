
using System;
using Godot;

public class PollingStatLabel : StatLabel
{
    private TimerAction _timer;
    private RefAction _timerRing;
    public static PollingStatLabel Construct<TProperty>(
        string name, Label label,
        Func<TProperty> getStat, float timerPeriod)
    {
        var d = new PollingStatLabel();
        d.Setup<TProperty>(name, label, getStat, timerPeriod);
        return d;
    }
    protected void Setup<TProperty>(
        string name, Label label,
        Func<TProperty> getStat, float timerPeriod)
    {
        _timerRing = new RefAction();
        _timer = new TimerAction(timerPeriod, timerPeriod, _timerRing.Invoke);
        base.Setup<TProperty>(name, label, getStat, _timerRing);
    }

    public override void _Process(float delta)
    {
        _timer.Process(delta);
    }
}
