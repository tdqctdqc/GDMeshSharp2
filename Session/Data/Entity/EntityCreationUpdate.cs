using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;

[MessagePackObject(keyAsPropertyName: true)] 

public sealed partial class EntityCreationUpdate : Update
{
    public string EntityTypeName { get; private set; }
    public byte[] EntityBytes { get; private set; }
    
    public static EntityCreationUpdate Create(Entity entity, HostWriteKey key)
    {
        var entityBytes = Game.I.Serializer.MP.Serialize(entity, entity.GetType());

        return new EntityCreationUpdate(entity.GetType().Name, entityBytes);
    }
    [SerializationConstructor] public EntityCreationUpdate(string entityTypeName, byte[] entityBytes) 
    {
        EntityBytes = entityBytes;
        EntityTypeName = entityTypeName;
    }
    public override void Enact(ServerWriteKey key)
    {
        var eType = Game.I.Serializer.Types[EntityTypeName];
        var e = (Entity)Game.I.Serializer.MP.Deserialize(EntityBytes, eType);
        e.GetMeta().AddToData(e, key);
    }
}
