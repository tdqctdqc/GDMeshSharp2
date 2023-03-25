
using System;
using System.Collections;
using System.Collections.Generic;

public static class NoticeHandler
{
    public static Action Clear { get; set; }
}
public abstract class NoticeHandler<TNotice> 
{
    private static Action<TNotice> _action;
    private static Action<TNotice> _oneTime;

    static NoticeHandler()
    {
        //todo fix 

        _action = n => { };
        _oneTime = n => { };
        NoticeHandler.Clear += Clear;
    }

    private static void Clear()
    {
        _action = n => { };
        _oneTime = n => { };
        NoticeHandler.Clear -= Clear;
    }
    protected static void RaiseBase(TNotice notice)
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
