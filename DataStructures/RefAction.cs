
using System;
using System.Collections.Generic;

public class RefAction
{
    private Action _action;

    public void Invoke()
    {
        _action?.Invoke();
    }

    public void Subscribe(Action a)
    {
        _action += a;
    }

    public void Unsubscribe(Action a)
    {
        _action -= a;
    }
}
public class RefAction<TArg>
{
    private Action<TArg> _action;
    private HashSet<RefAction<TArg>> _subscribingTo;
    public void Invoke(TArg t)
    {
        _action?.Invoke(t);
    }
    
    public void Subscribe(RefAction<TArg> a)
    {
        _action += a.Invoke;
        if (a._subscribingTo == null) a._subscribingTo = new HashSet<RefAction<TArg>>();
        a._subscribingTo.Add(this);
    }
    public void Subscribe(Action<TArg> a)
    {
        _action += a.Invoke;
    }
    public void Unsubscribe(RefAction<TArg> a)
    {
        _action -= a.Invoke;
    }
    public void Unsubscribe(ref Action<TArg> a)
    {
        _action -= a.Invoke;
    }
    public void EndSubscriptions()
    {
        if (_subscribingTo == null) return;
        foreach (var refAction in _subscribingTo)
        {
            refAction.Unsubscribe(this);
        }
    }
}