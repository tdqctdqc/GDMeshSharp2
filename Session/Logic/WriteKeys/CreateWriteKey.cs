using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CreateWriteKey : StrongWriteKey
{
    public CreateWriteKey(Data data) : base(data)
    {
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        t.GetMeta().AddToData(t, this);
    }
}