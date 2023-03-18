
using System;

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
public class RefAction<T>
{
    private Action<T> _action;

    public void Invoke(T t)
    {
        _action?.Invoke(t);
    }

    public void Subscribe(Action<T> a)
    {
        _action += a;
    }

    public void Unsubscribe(Action<T> a)
    {
        _action -= a;
    }
}