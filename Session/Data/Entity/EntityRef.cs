using Godot;
using System;
using System.Collections.Generic;

public class EntityRef<TRef> : IRef where TRef : Entity
{
    public int RefId { get; private set; }
    private TRef _ref;
    
    public static EntityRef<TRef> Construct(TRef entity, CreateWriteKey key)
    {
        var refer = new EntityRef<TRef>(entity.Id);
        refer._ref = entity;
        return refer;
    }
    public EntityRef(int refId)
    {
        RefId = refId;
        _ref = null;
    }

    public void Set(int id, CreateWriteKey key)
    {
        RefId = id;
        _ref = (TRef)key.Data[RefId];
    }
    public TRef Ref()
    {
        if (RefId == -1) return null;
        if (_ref == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }
        return _ref;
    }

    public bool Empty()
    {
        return RefId == -1;
    }
    public void SyncRef(Data data)
    {
        try
        {
            _ref = (TRef) data[RefId];
        }
        catch (Exception e)
        {
            GD.Print(data[RefId].GetType().ToString() + " supposed to be " + typeof(TRef).ToString());
            throw;
        }
    }
}
