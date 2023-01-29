using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

public class StateTransferUpdate : Update
{
    public Dictionary<string, Dictionary<string, List<object[]>>> Data { get; private set; }

    public static void Send(HostWriteKey key, PacketPeerStream hostPacket)
    {
        foreach (var kvpDomain in key.Data.Domains)
        {
            var domainType = kvpDomain.Key;
            var domain = kvpDomain.Value;
            var repos = domain.Repos.ToList();
            repos.ForEach(r => GD.Print(r.Key.Name));
            for (var i = 0; i  < repos.Count; i++)
            {
                var kvpRepo = repos[i];
                var entityType = kvpRepo.Key;
                
                var repo = kvpRepo.Value;
                int iter = 0;
                foreach (var e in repo.Entities)
                {
                    var u = new EntityCreationUpdate(e.GetType(), domainType, e, key);
                    var wrapperBytes = u.GetPacketBytes();
                    var err = hostPacket.PutPacket(wrapperBytes);
                    if (err != Error.Ok)
                    {
                        return;
                    }
                }
                GD.Print("finished entities " + entityType.Name);
            }
        }
        var done = new FinishedStateTransferUpdate(key);
        var doneBytes = done.GetPacketBytes();
        hostPacket.PutPacket(doneBytes);
    }
    private StateTransferUpdate(HostWriteKey key) : base(key)
    {
        
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
                    //todo revert
                    // var entity = meta.Deserialize(eJson, key);
                    // key.Data.AddEntity(entity, domain.GetType(), key);
                });
            }
        }
    }
}
