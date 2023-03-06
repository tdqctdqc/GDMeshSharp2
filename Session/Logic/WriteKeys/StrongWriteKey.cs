using Godot;
using System;

public class StrongWriteKey : WriteKey
{
    public StrongWriteKey(Data data, ISession session) : base(data, session)
    {
    }

    public void Delete<TEntity>(TEntity t) where TEntity : Entity
    {
        t.GetMeta().RemoveFromData(t, this);
    }
}
