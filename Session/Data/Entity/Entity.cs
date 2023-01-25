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
    protected Entity(object[] args)
    {        
        GetMeta().Initialize(this, args);
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
        GetMeta().UpdateEntityRefVar<TUnderlying>(fieldName, this, key, newValue);
        if (key is HostWriteKey hKey)
        {
            EntityVarUpdate.Send(fieldName, Id, newValue, hKey);
        }
    }
}
