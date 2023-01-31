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
    public string DomainTypeName { get; private set; }
    public byte[] EntityBytes { get; private set; }
    
    public static void Send(Entity entity, Type domainType, HostWriteKey key)
    {
        var u = new EntityCreationUpdate(entity.GetType(), domainType, entity, key);
        key.HostServer.QueueUpdate(u);
    }
    public EntityCreationUpdate(string entityTypeName, string domainTypeName, byte[] entityBytes) 
        : base(new HostWriteKey(null, null))
    {
        EntityBytes = entityBytes;
        EntityTypeName = entityTypeName;
        DomainTypeName = domainTypeName;
    }
    public EntityCreationUpdate(Type entityType, Type domainType, Entity e, HostWriteKey key) : base(key)
    {
        EntityTypeName = entityType.Name;
        DomainTypeName = domainType.Name;
        EntityBytes = Game.I.Serializer.MP.Serialize(e, entityType);
    }
    public override void Enact(ServerWriteKey key)
    {
        var entityType = Game.I.Serializer.Types[EntityTypeName];
        var domainType = Game.I.Serializer.Types[DomainTypeName];
        var e = Game.I.Serializer.MP.Deserialize<Entity>(EntityBytes);
        key.Data.AddEntity(e, domainType, key);
    }
}
