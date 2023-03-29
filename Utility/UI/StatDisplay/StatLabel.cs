
using System;
using Godot;

public class StatLabel : Node
{
    private Label _label;
    private Action _update;
    protected void Setup<TProperty>(
        string name, Label label,
        Func<TProperty> getStat,
        RefAction trigger)
    {
        _label = label;
        if (_label == null) throw new Exception();
        _label.AddChild(this);
        _update = () =>
        { 
            _label.Text = $"{name}: {getStat().ToString()}";
        };
        trigger.Subscribe(_update);
    }

    protected void TriggerUpdate()
    {
        _update.Invoke();
    }
}
