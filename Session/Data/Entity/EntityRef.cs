using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public interface IEntityRef : IRef
{
    int RefId { get; }
}
public class EntityRef<TRef> : IEntityRef where TRef : Entity
{
    public int RefId { get; private set; }
    private TRef _ref;
    
    public EntityRef(TRef entity, CreateWriteKey key)
    {
        RefId = entity.Id;
        key.Data.RefFulfiller.Fulfill(this);
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

    public bool Fulfilled()
    {
        return RefId != -1;
    }

    public bool CheckExists(Data data)
    {
        if (data.Entities.TryGetValue(RefId, out var e))
        {
            return e is TRef;
        }

        return false;
    }
    public void SyncRef(Data data)
    {
        if (data.Entities.ContainsKey(RefId))
        {
            _ref = (TRef) data[RefId];
        }
        else
        {
            ClearRef();
        }
    }

    public void ClearRef()
    {
        RefId = -1;
        _ref = null;
    }
    
    public override string ToString()
    {
        return Empty() ? "Empty" : Entity().ToString();
    }
}
