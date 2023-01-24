using Godot;
using System;
using System.Collections.Generic;

public class EntityCreationUpdate : IUpdate
{
    string IUpdate.UpdateType => UpdateType;
    public static string UpdateType => "EntityCreation";
    public Type EntityType { get; private set; }
    public Type DomainType { get; private set; }
    public string EntityJson { get; private set; }

    public static EntityCreationUpdate Encode(Entity entity, Type domainType, HostWriteKey key)
    {
        var entityJson = entity.GetMeta().Serialize(entity);
        return new EntityCreationUpdate(entity.GetType(), domainType, entityJson);
    }
    private EntityCreationUpdate(Type entityType, Type domainType, string entityJson)
    {
        EntityJson = entityJson;
        EntityType = entityType;
        DomainType = domainType;
    }
    public string Serialize()
    {
        var jsonArray = new System.Text.Json.Nodes.JsonArray();
        jsonArray.Add(EntityType.Name);
        jsonArray.Add(DomainType.Name);
        jsonArray.Add(EntityJson);
        return jsonArray.ToJsonString();
    }
    public static void DeserializeAndEnact(string json, ServerWriteKey key)
    {
        var list = Serializer.Deserialize<List<string>>(json);
        var entityType = Serializer.EntityTypes[list[0]];
        var domainType = Serializer.DomainTypes[list[1]];
        var entityJson = list[2];
        var entityMeta = Serializer.GetEntityMeta(entityType);
        var entity = entityMeta.Deserialize(entityJson);
        key.Data.AddEntity(entity, domainType, key);
    }
}
