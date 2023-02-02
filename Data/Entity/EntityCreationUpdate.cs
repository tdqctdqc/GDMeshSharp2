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
    
    public static void Send<TEntity>(TEntity entity, HostWriteKey key)
    {
        var entityBytes = Game.I.Serializer.MP.Serialize<TEntity>(entity);

        var u = new EntityCreationUpdate(entity.GetType().Name, entityBytes);

        // key.HostServer.QueueUpdate(u);
    }
    public EntityCreationUpdate(string entityTypeName, byte[] entityBytes) 
        : base(new HostWriteKey(null, null))
    {
        EntityBytes = entityBytes;
        EntityTypeName = entityTypeName;
    }
    public static EntityCreationUpdate GetForTest(Entity e, HostWriteKey key)
    {
        return new EntityCreationUpdate(e.GetType().Name, Game.I.Serializer.MP.Serialize(e, e.GetType()));
    }
    public override void Enact(ServerWriteKey key)
    {
        var e = Game.I.Serializer.MP.Deserialize<Entity>(EntityBytes);
        e.GetMeta().AddToData(e, key);
    }
}
