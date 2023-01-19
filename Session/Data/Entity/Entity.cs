using Godot;
using System;
using System.Collections.Generic;

public abstract class Entity
{
    public int Id { get; protected set; }
    public IEntityMeta GetMeta() => Serializer.GetEntityMeta(GetType());
    
    protected Entity(int id, CreateWriteKey key) : base()
    {
        Id = id;
    }
    protected Entity(string json)
    {        
        GetMeta().Initialize(this, json);
    }
    public void Set<TValue>(string fieldName, TValue newValue, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<TValue>(fieldName, this, key, newValue);
        if (key is HostWriteKey hKey)
        {
            hKey.Server.QueueUpdate(EntityVarUpdate.Encode(fieldName, Id, newValue, hKey));
        }
    }
}
