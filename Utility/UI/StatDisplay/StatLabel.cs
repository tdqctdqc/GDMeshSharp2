
using System;
using Godot;

public class StatLabel : Node
{
    private Label _label;
    private Func<string> _update;
    protected void Setup<TEntity, TProperty>(TEntity e,
        string name, Label label,
        Func<TEntity, TProperty> getStat)
    {
        _label = label;
        if (_label == null) throw new Exception();
        _label.AddChild(this);
        _update = () =>
        {
            return _label.Text = $"{name}: {getStat(e).ToString()}";
        };
    }

    protected void TriggerUpdate()
    {
        _update.Invoke();
    }
}
