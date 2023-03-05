using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

[RefAttribute] 
[MessagePackObject(keyAsPropertyName: true)] 
public class EntityRef<TRef> : IRef<int> where TRef : Entity
{
    public int RefId { get; private set; }
    private TRef _ref;
    
    public EntityRef(TRef entity, CreateWriteKey key)
    {
        RefId = entity.Id;
        _ref = entity;
    }
    public EntityRef(int refId)
    {
        RefId = refId;
        _ref = null;
    }
    public TRef Entity()
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

    public bool Check(Data data)
    {
        if (data.Entities.TryGetValue(RefId, out var e))
        {
            return e is TRef;
        }

        return false;
    }
    public void SyncRef(Data data)
    {
        _ref = (TRef) data[RefId];
    }
}
