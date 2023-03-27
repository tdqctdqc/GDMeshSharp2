
using System;
using Godot;

public class SubscribedStatLabel : StatLabel
{
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
        base.Setup<TEntity, TProperty>(e, name, label, getStat);
    }
    private void SetupForEntityDynamic<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat) where TEntity : Entity
    {
        var r = new RefAction<ValChangeNotice<TProperty>>();
        r.Subscribe(n => TriggerUpdate());

        Game.I.Client.Requests.SubscribeForValChangeSpecific<TEntity, TProperty>(name, e, r);
        

        base.Setup<TEntity, TProperty>(e, name, label, getStat);
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
    }
}
