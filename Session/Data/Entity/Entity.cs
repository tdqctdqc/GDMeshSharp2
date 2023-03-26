using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public abstract class Entity
{
    public int Id { get; protected set; }
    public IEntityMeta GetMeta() => Game.I.Serializer.GetEntityMeta(GetType());
    protected Entity(int id)
    {
        Id = id;  
    }

    public void Set<TValue>(string fieldName, TValue newValue, StrongWriteKey key)
    {
        GetMeta().UpdateEntityVar<TValue>(fieldName, this, key, newValue);
    }
    public abstract Type GetDomainType();
}
