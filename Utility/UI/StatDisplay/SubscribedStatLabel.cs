
using System;
using Godot;

public class SubscribedStatLabel : StatLabel
{
    private SubscribedStatLabel()
    {
        
    }
    public static SubscribedStatLabel Construct<TProperty>(
        string name, Label label,
        Func<TProperty> getStat, RefAction trigger)
    {
        var d = new SubscribedStatLabel();
        d.Setup<TProperty>(name, label, getStat, trigger);
        return d;
    }
    private void Setup<TProperty>(
        string name, Label label,
        Func<TProperty> getStat, RefAction trigger)
    {
        trigger.Subscribe(TriggerUpdate);
        base.Setup<TProperty>(name, label, getStat, trigger);
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
    }
}
