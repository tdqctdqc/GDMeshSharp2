using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

[RefAttribute] public class EntityRef<TRef> : IRef<int> where TRef : Entity
{
    public int RefId { get; private set; }
    private TRef _ref;
    
    public EntityRef(TRef entity, CreateWriteKey key)
    {
        RefId = entity.Id;
        _ref = entity;
    }
    [JsonConstructor] public EntityRef(int refId)
    {
        RefId = refId;
        _ref = null;
    }

    public void Set(int id, StrongWriteKey key)
    {
        RefId = id;
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
        _ref = (TRef) data[RefId];
    }
    public static EntityRef<TRef> DeserializeConstructor(int t)
    {
        return new EntityRef<TRef>(t);
    }
    public int GetUnderlying() => RefId;
}
