using Godot;
using System;
using System.Collections.Generic;

public abstract class Entity
{
    public int Id { get; protected set; }
    public IEntityMeta GetMeta() => Game.I.Serializer.GetEntityMeta(GetType());
    
    protected Entity(int id, CreateWriteKey key) : base()
    {
        Id = id;
    }

    protected Entity(int id)
    {
        Id = id;
    }
    protected Entity(object[] args, ServerWriteKey key)
    {        
        GetMeta().Initialize(this, args, key);
    }
    public void Set<TValue>(string fieldName, TValue newValue, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<TValue>(fieldName, this, key, newValue);
        if (key is HostWriteKey hKey)
        {
            EntityVarUpdate.Send(fieldName, Id, newValue, hKey);
        }
    }
    public void SetRef<TUnderlying>(string fieldName, TUnderlying newValue, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<TUnderlying>(fieldName, this, key, newValue);
        if (key is HostWriteKey hKey)
        {
            EntityVarUpdate.Send(fieldName, Id, newValue, hKey);
        }
    }
}
