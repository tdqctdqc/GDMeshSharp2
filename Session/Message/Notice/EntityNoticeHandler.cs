
using System;
using System.Linq;
using System.Reflection;

public class EntityNoticeHandler<TEntity, TNotice> : NoticeHandler<TNotice>
    where TEntity : Entity
{
    protected static void Raise(TNotice notice)
    {
        //todo handle bubbling somehow
        RaiseBase(notice);
    }
}
