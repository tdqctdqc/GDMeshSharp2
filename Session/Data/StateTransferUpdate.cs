using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateTransferUpdate : Update
{
    public Dictionary<string, Dictionary<string, List<object[]>>> Data { get; private set; }

    public static void Send(HostWriteKey key, StreamPeerTCP connection)
    {
        foreach (var kvpDomain in key.Data.Domains)
        {
            var domainType = kvpDomain.Key;
            var domain = kvpDomain.Value;
            foreach (var kvpRepo in domain.Repos)
            {
                var entityType = kvpRepo.Key;
                var repo = kvpRepo.Value;
                foreach (var e in repo.Entities)
                {
                    EntityCreationUpdate.Send(e, domainType, key, connection);
                }
            }
        }
    }

    private StateTransferUpdate(HostWriteKey key) : base(key)
    {
        Data = new Dictionary<string, Dictionary<string, List<object[]>>>();
        foreach (var keyValuePair in key.Data.Domains)
        {
            var domainType = keyValuePair.Key;
            var domain = keyValuePair.Value;
            var dic = new Dictionary<string, List<object[]>>();
            Data.Add(domainType.Name, dic);
            
            foreach (var kvp in domain.Repos)
            {
                var entityType = kvp.Key;
                var meta = Game.I.Serializer.GetEntityMeta(entityType);
                var repo = kvp.Value;
                var eArgs = new List<object[]>();                    
                dic.Add(entityType.Name, eArgs);

                foreach (var entity in repo.Entities)
                {
                    eArgs.Add(meta.GetArgs(entity));
                }
            }
        }
    }
    public override void Enact(ServerWriteKey key)
    {
        foreach (var keyValuePair in Data)
        {
            var domainName = keyValuePair.Key;
            var domain = key.Data.GetDomain(domainName);
            var domainEntityTypes = keyValuePair.Value;
            foreach (var domainEntityType in domainEntityTypes)
            {
                var entityTypeName = domainEntityType.Key;
                var entityJsons = domainEntityType.Value;
                var meta = Game.I.Serializer.GetEntityMeta(entityTypeName);
                entityJsons.ForEach(eJson =>
                {
                    var entity = meta.Deserialize(eJson);
                    key.Data.AddEntity(entity, domain.GetType(), key);
                });
            }
        }
    }

    private static StateTransferUpdate DeserializeConstructor(object[] args)
    {
        return new StateTransferUpdate(args);
    }

    private StateTransferUpdate(object[] args) : base(args) {}
}
