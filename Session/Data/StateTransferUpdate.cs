using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateTransferUpdate : IUpdate
{
    string IUpdate.UpdateType => UpdateType;
    public static string UpdateType = "StateTransfer";
    public Dictionary<string, Dictionary<string, List<string>>> Data { get; private set; }

    public static StateTransferUpdate Encode(HostWriteKey key)
    {
        return new StateTransferUpdate(key);
    }

    private StateTransferUpdate(HostWriteKey key)
    {
        Data = new Dictionary<string, Dictionary<string, List<string>>>();
        foreach (var keyValuePair in key.Data.Domains)
        {
            var domainType = keyValuePair.Key;
            var domain = keyValuePair.Value;
            var dic = new Dictionary<string, List<string>>();
            Data.Add(domainType.Name, dic);
            
            foreach (var kvp in domain.Repos)
            {
                var entityType = kvp.Key;
                var meta = Serializer.GetEntityMeta(entityType);
                var repo = kvp.Value;
                var eJsons = new List<string>();                    
                dic.Add(entityType.Name, eJsons);

                foreach (var entity in repo.Entities)
                {
                    eJsons.Add(meta.Serialize(entity));
                }
            }
        }
    }
    public string Serialize()
    {
        return Serializer.Serialize(Data);
    }

    public static void DeserializeAndEnact(string json, ServerWriteKey key)
    {
        var deserialized = Serializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json);
        foreach (var keyValuePair in deserialized)
        {
            var domainName = keyValuePair.Key;
            var domain = key.Data.GetDomain(domainName);
            var domainEntityTypes = keyValuePair.Value;
            foreach (var domainEntityType in domainEntityTypes)
            {
                var entityTypeName = domainEntityType.Key;
                var entityType = Serializer.EntityTypes[entityTypeName];
                var entityJsons = domainEntityType.Value;
                var meta = Serializer.GetEntityMeta(entityType);
                entityJsons.ForEach(eJson =>
                {
                    var entity = meta.Deserialize(eJson);
                    key.Data.AddEntity(entity, domain.GetType(), key);
                });
                
            }
        }
    }
}
