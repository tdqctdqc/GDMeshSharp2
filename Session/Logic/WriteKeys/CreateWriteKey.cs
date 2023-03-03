using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CreateWriteKey : StrongWriteKey
{
    public IdDispenser IdDispenser { get; private set; }
    public CreateWriteKey(Data data) : base(data)
    {
        data.GetIdDispenser(this);
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        t.GetMeta().AddToData(t, this);
    }

    public void SetIdDispenser(IdDispenser id)
    {
        IdDispenser = id;
    }
}