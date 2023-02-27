using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RefFulfiller
{
    private Data _data;

    public RefFulfiller(Data data)
    {
        _data = data;
    }


    public void Fulfill(IRef r)
    {
        r.SyncRef(_data);
    }

    public TEntity Get<TEntity>(int id) where TEntity : Entity
    {
        return (TEntity) _data.Entities[id];
    }
}