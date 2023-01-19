using Godot;
using System;
using System.Collections.Generic;

public abstract class Entity
{
    public EntityId Id { get; private set; }
    public IEntityMeta GetMeta() => Serializer.GetEntityMeta(GetType());
    
    protected Entity(int id, HostWriteKey key) : base()
    {
        Id = EntityId.Construct(id);
    }
    protected Entity(string json)
    {        
        GetMeta().Initialize(this, json);
    }
}
