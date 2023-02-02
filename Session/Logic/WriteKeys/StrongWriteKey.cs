using Godot;
using System;

public class StrongWriteKey : WriteKey
{
    public StrongWriteKey(Data data) : base(data)
    {
    }

    public void Delete<TEntity>(TEntity t) where TEntity : Entity
    {
        t.GetMeta().RemoveFromData(t, this);
    }
}
