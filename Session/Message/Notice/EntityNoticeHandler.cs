
using System;

public class EntityNoticeHandler<TEntity, TNotice> : NoticeHandler<TNotice>
    where TEntity : Entity
{
    //todo put notice bubbling logic here?
    protected static void Raise(TNotice notice)
    {
        RaiseBase(notice);
    }
}
