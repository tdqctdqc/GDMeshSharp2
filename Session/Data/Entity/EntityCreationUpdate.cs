using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

public sealed class EntityCreationUpdate : Update
{
    public string EntityTypeName { get; private set; }
    public string DomainTypeName { get; private set; }
    public byte[] EntityBytes { get; private set; }
    
    public static void Send(Entity entity, Type domainType, HostWriteKey key)
    {
        var u = new EntityCreationUpdate(entity.GetType(), domainType, entity, key);
        key.HostServer.QueueUpdate(u);
    }
    [JsonConstructor] public EntityCreationUpdate(string entityTypeName, string domainTypeName, byte[] entityBytes) 
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
        EntityBytes = Game.I.Serializer.SerializeToUtf8(e);
    }
    public override void Enact(ServerWriteKey key)
    {
        var entityType = Game.I.Serializer.Types[EntityTypeName];
        var domainType = Game.I.Serializer.Types[DomainTypeName];
        var e = (Entity)Game.I.Serializer.Deserialize(EntityBytes, entityType);
        key.Data.AddEntity(e, domainType, key);
    }
}
