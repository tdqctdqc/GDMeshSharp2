
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class NoticeHandler<TNotice> 
{
    protected static Action Clear { get; set; }
    private static Action<TNotice> _action;
    private static Action<TNotice> _oneTime;

    static NoticeHandler()
    {
        //todo fix 
        Game.I.NewSession += () => Clear?.Invoke();

        _action = n => { };
        _oneTime = n => { };
        Clear += () =>
        {
            // _action = n => { };
            // _oneTime = n => { };
        };
    }
    public static void Raise(TNotice notice)
    {
        _action?.Invoke(notice);
        _oneTime?.Invoke(notice);
        _oneTime = n => { };
    }

    public static void Register(Action<TNotice> callback)
    {
        _action += callback;
    }
    public static void RegisterOneTime(Action<TNotice> callback)
    {
        _oneTime += callback;
    }
}
