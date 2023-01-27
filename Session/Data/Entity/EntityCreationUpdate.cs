using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

public sealed class EntityCreationUpdate : Update
{
    public string EntityType { get; private set; }
    public string DomainType { get; private set; }
    public byte[] EntityBytes { get; private set; }
    
    public static void Send(Entity entity, Type domainType, HostWriteKey key)
    {
        var u = new EntityCreationUpdate(entity.GetType(), domainType, entity, key);
        key.HostServer.QueueUpdate(u);
    }
    public static void Send(Entity entity, Type domainType, HostWriteKey key, StreamPeerTCP connection)
    {
        var entityArgs = entity.GetMeta().GetArgs(entity);
        var u = new EntityCreationUpdate(entity.GetType(), domainType, entity, key);
        var args = u.GetMeta().GetArgs(u);
        connection.PutVar(args);
    }
    [JsonConstructor] public EntityCreationUpdate(string entityType, string domainType, byte[] entityBytes) : base(new HostWriteKey(null, null))
    {
        EntityBytes = entityBytes;
        EntityType = entityType;
        DomainType = domainType;
    }
    public EntityCreationUpdate(Type entityType, Type domainType, Entity e, HostWriteKey key) : base(key)
    {
        EntityType = entityType.Name;
        DomainType = domainType.Name;
        EntityBytes = Game.I.Serializer.SerializeToUtf8(e);
    }
    public override void Enact(ServerWriteKey key)
    {
        var entityType = Game.I.Serializer.Types[EntityType];
        GD.Print("trying to deserialize entity type " + entityType.Name);
        var e = Game.I.Serializer.Deserialize(EntityBytes, entityType);
        // var entityMeta = Game.I.Serializer.GetEntityMeta(EntityType);
        // var eArgsB = Game.I.Serializer.Deserialize<byte[][]>(EntityArgs);
        // for (var i = 0; i < eArgsA.Length; i++)
        // {
        //     eArgs.Add((byte[])eArgsA[i]);
        // }
        // var entity = entityMeta.Deserialize(eArgsB.ToArray(), key);
        // var domainType = Game.I.Serializer.DomainTypes[DomainType];
        // key.Data.AddEntity(entity, domainType, key);
    }
    private static EntityCreationUpdate DeserializeConstructor(object[] args)
    {
        return new EntityCreationUpdate(args);
    }
    private EntityCreationUpdate(object[] args) : base(args) {}
}
