
using System;
using Godot;

public class SubscribedStatLabel : StatLabel
{
    private Action _unsubscribe;
    private SubscribedStatLabel()
    {
        
    }
    public static SubscribedStatLabel ConstructForEntityTrigger<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat, RefAction trigger)
    {
        var d = new SubscribedStatLabel();
        d.SetupForEntityTrigger(e, name, label, getStat, trigger);
        return d;
    }
    public static SubscribedStatLabel ConstructForEntityDynamic<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat) where TEntity : Entity
    {
        var d = new SubscribedStatLabel();
        d.SetupForEntityDynamic(e, name, label, getStat);
        return d;
    }
    
    private void SetupForEntityTrigger<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat, RefAction trigger)
    {
        trigger.Subscribe(TriggerUpdate);
        _unsubscribe = () => trigger.Unsubscribe(TriggerUpdate);
        base.Setup<TEntity, TProperty>(e, name, label, getStat);
    }
    
    private void SetupForEntityDynamic<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat) where TEntity : Entity
    {
        Action<ValChangeNotice<TProperty>> act = n => TriggerUpdate();
        Game.I.Client.Requests.RegisterForSpecific(name, e, act);
        base.Setup<TEntity, TProperty>(e, name, label, getStat);
    }
    
    public override void _ExitTree()
    {
        _unsubscribe();
        base._ExitTree();
    }
}
