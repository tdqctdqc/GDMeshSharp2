
using System;
using System.Collections.Generic;
using Godot;

public class RefAction
{
    private Action _action;
    private HashSet<RefAction> _subscribingTo;
    
    public void Invoke()
    {
        _action?.Invoke();
    }

    public void Clear()
    {
        _action = () => { };
        EndSubscriptions();
    }
    public void Subscribe(Action a)
    {
        _action += a;
    }
    public void Subscribe(RefAction a)
    {
        _action += a.Invoke;
        if (a._subscribingTo == null) a._subscribingTo = new HashSet<RefAction>();
        a._subscribingTo.Add(this);
    }
    public void Unsubscribe(Action a)
    {
        _action -= a;
    }
    public void Unsubscribe(RefAction a)
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
        _subscribingTo.Clear();
    }
}
public class RefAction<TArg>
{
    public RefAction Blank { get; private set; }
    public RefAction()
    {
        Blank = new RefAction();
        _action += t => Blank.Invoke();
    }
    private Action<TArg> _action;
    private HashSet<RefAction<TArg>> _subscribingTo;
    public int Subscribers { get; private set; }
    public void Invoke(TArg t)
    {
        if (Subscribers > 0) _action?.Invoke(t);
        Blank.Invoke();
    }
    public void Clear()
    {
        _action = t => { };
        EndSubscriptions();
    }
    public void Subscribe(RefAction a)
    {
        Blank.Subscribe(a);
    }
    public void Subscribe(RefAction<TArg> a)
    {
        _action += a.Invoke;
        if (a._subscribingTo == null) a._subscribingTo = new HashSet<RefAction<TArg>>();
        a._subscribingTo.Add(this);
        Subscribers++;
    }
    public void Subscribe(Action<TArg> a)
    {
        _action += a.Invoke;
        Subscribers++;
    }
    public void Unsubscribe(RefAction<TArg> a)
    {
        _action -= a.Invoke;
    }
    public void Unsubscribe(ref Action<TArg> a)
    {
        _action -= a.Invoke;        
    }
    public void Unubscribe(RefAction a)
    {
        Blank.Unsubscribe(a);
    }
    public void EndSubscriptions()
    {
        if (_subscribingTo == null) return;
        foreach (var refAction in _subscribingTo)
        {
            refAction.Unsubscribe(this);
        }
        _subscribingTo.Clear();
    }
}