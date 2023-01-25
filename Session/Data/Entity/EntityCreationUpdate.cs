using Godot;
using System;
using System.Collections.Generic;

public class EntityCreationUpdate : Update
{
    public string EntityType { get; private set; }
    public string DomainType { get; private set; }
    public object[] EntityArgs { get; private set; }

    public static void Send(Entity entity, Type domainType, HostWriteKey key)
    {
        var entityJson = entity.GetMeta().Serialize(entity);
        var u = new EntityCreationUpdate(entity.GetType(), domainType, entity, key);
    }
    private EntityCreationUpdate(Type entityType, Type domainType, Entity e, HostWriteKey key) : base(key)
    {
        EntityType = entityType.Name;
        DomainType = domainType.Name;
        EntityArgs = e.GetMeta().Serialize(e);
    }
    public override void Enact(ServerWriteKey key)
    {
        var entityMeta = Game.I.Serializer.GetEntityMeta(EntityType);
        var entity = entityMeta.Deserialize(EntityArgs);
        var domainType = Game.I.Serializer.DomainTypes[DomainType];
        key.Data.AddEntity(entity, domainType, key);
    }
    private static EntityCreationUpdate DeserializeConstructor(object[] args)
    {
        return new EntityCreationUpdate(args);
    }
    private EntityCreationUpdate(object[] args) : base(args) {}
}
