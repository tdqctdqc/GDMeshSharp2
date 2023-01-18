using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateTransferUpdate : IUpdate
{
    string IUpdate.UpdateType => UpdateType;
    public static string UpdateType = "StateTransfer";
    public List<Type> DomainTypes { get; private set; }
    public List<string> EntityJsons { get; private set; }
    public List<Type> EntityTypes { get; private set; }

    public static StateTransferUpdate Encode(HostWriteKey key)
    {
        return new StateTransferUpdate();
    }

    private StateTransferUpdate()
    {
        DomainTypes = new List<Type>();
        EntityJsons = new List<string>();
        EntityTypes = new List<Type>();
        foreach (var keyValuePair in Game.I.Session.Data.Domains)
        {
            var domainType = keyValuePair.Key;
            var domain = keyValuePair.Value;
            foreach (var kvp in domain.Repos)
            {
                var entityType = kvp.Key;
                var meta = Serializer.GetEntityMeta(entityType);
                var repo = kvp.Value;
                foreach (var entity in repo.Entities)
                {
                    DomainTypes.Add(domainType);
                    EntityJsons.Add(meta.Serialize(entity));
                    EntityTypes.Add(entityType);
                }
            }
        }
    }
    public string Serialize()
    {
        var jsonArray = new System.Text.Json.Nodes.JsonArray();
        var domainTypes = System.Text.Json.JsonSerializer.Serialize<List<string>>(DomainTypes.Select(dt => dt.Name).ToList());
        jsonArray.Add(domainTypes);
        var entityJsons = System.Text.Json.JsonSerializer.Serialize<List<string>>(EntityJsons);
        jsonArray.Add(entityJsons);
        var entityTypes = System.Text.Json.JsonSerializer.Serialize<List<string>>(EntityTypes.Select(et => et.Name).ToList());
        jsonArray.Add(entityTypes);
        return jsonArray.ToJsonString();
    }

    public static void DeserializeAndEnact(string json, ServerWriteKey key)
    {
        var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
        var domainTypesJsons = System.Text.Json.JsonSerializer.Deserialize<List<string>>(list[0]);
        var domainTypes = domainTypesJsons.Select(dj => Serializer.DomainTypes[dj]).ToList();
        var entityJsons = System.Text.Json.JsonSerializer.Deserialize<List<string>>(list[1]);
        var entityTypeJsons = System.Text.Json.JsonSerializer.Deserialize<List<string>>(list[2]);
        var entityTypes = entityTypeJsons.Select(ej => Serializer.EntityTypes[ej]).ToList();
        
        
        for (var i = 0; i < domainTypes.Count; i++)
        {
            var domainType = domainTypes[i];
            var meta = Serializer.GetEntityMeta(entityTypes[i]);
            var entity = meta.Deserialize(entityJsons[i]);
            Game.I.Session.Data.AddEntity(entity, domainType, key);
        }
    }
}
