using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

public class StateTransferUpdate : Update
{
    public Dictionary<string, Dictionary<string, List<object[]>>> Data { get; private set; }

    public static void Test(HostWriteKey key, StreamPeerTCP hostConnection, PacketPeerStream remotePacket, 
        PacketPeerStream hostPacket)
    {
        foreach (var kvpDomain in key.Data.Domains)
        {
            var domainType = kvpDomain.Key;
            var domain = kvpDomain.Value;
            var repos = domain.Repos.ToList();
            repos.ForEach(r => GD.Print(r.Key.Name));
            int bytesSent = 0;
            for (var i = 0; i  < repos.Count; i++)
            {
                var kvpRepo = repos[i];
                var entityType = kvpRepo.Key;
                GD.Print("sending entities " + entityType.Name);
                
                var repo = kvpRepo.Value;
                GD.Print("count " + repo.Entities.Count);

                // if (repo.Entities.Count > 0)
                // {
                //     Check(repo.Entities[0], connection);
                // }
                int iter = 0;
                foreach (var e in repo.Entities)
                {
                    // EntityCreationUpdate.Send(e, domainType, key, connection);
                    
                    var u = new EntityCreationUpdate(e.GetType(), domainType, e, key);
                    var args = u.GetMeta().GetArgs(u);

                    var bytes = JsonSerializer.SerializeToUtf8Bytes(args);
                    bytesSent += bytes.Length;
                    // var res = connection.PutPartialData(bytes);
                    // var sentBytes = (int)res[1];
                    // if(bytes.Count() != sentBytes) GD.Print("didnt send all bytes");
                    hostPacket.PutVar(bytes);
                    remotePacket.GetVar();
                    
                    if (remotePacket.StreamPeer.GetAvailableBytes() >= remotePacket.InputBufferMaxSize)
                    {
                        GD.Print("bytes sent " + bytesSent);
                        GD.Print($"overflow {remotePacket.StreamPeer.GetAvailableBytes()} / {remotePacket.InputBufferMaxSize} ");
                        throw new Exception();
                    }
                    // if(connection.GetAvailableBytes() > 0) GD.Print("have " + connection.GetAvailableBytes());
                    iter++;
                    if(iter % 100 == 0) GD.Print($"done {iter} out of {repo.Entities.Count()}");
                }
                GD.Print("finished entities " + entityType.Name);
            }
            foreach (var kvpRepo in domain.Repos)
            {
                
            }
        }
    }
public static void Send(HostWriteKey key, StreamPeerTCP hostConnection, 
        PacketPeerStream hostPacket)
    {
        foreach (var kvpDomain in key.Data.Domains)
        {
            var domainType = kvpDomain.Key;
            var domain = kvpDomain.Value;
            var repos = domain.Repos.ToList();
            repos.ForEach(r => GD.Print(r.Key.Name));
            int bytesSent = 0;
            var bf = new BinaryFormatter();
            for (var i = 0; i  < repos.Count; i++)
            {
                var kvpRepo = repos[i];
                var entityType = kvpRepo.Key;
                GD.Print("sending entities " + entityType.Name);
                
                var repo = kvpRepo.Value;
                GD.Print("count " + repo.Entities.Count);
                int iter = 0;
                foreach (var e in repo.Entities)
                {
                    var u = new EntityCreationUpdate(e.GetType(), domainType, e, key);
                    var args = u.GetMeta().GetArgs(u);
                    var argsBytes = args.Select(a => Game.I.Serializer.SerializeToUtf8(a)).ToArray();
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(argsBytes);
                    var err = hostPacket.PutPacket(bytes);
                    if (err != Error.Ok)
                    {
                        return;
                    }
                }
                GD.Print("finished entities " + entityType.Name);
            }
        }
    }
    private static void Check(Entity e, StreamPeerTCP conn)
    {
        GD.Print("checking " + e.GetType().Name);
        var args = e.GetMeta().GetArgs(e);
        for (var i = 0; i < args.Length; i++)
        {
            var argType = args[i].GetType();
            GD.Print("arg " + i + " " + argType.Name);
            if (argType.IsGenericType)
            {
                for (var j = 0; j < argType.GenericTypeArguments.Length; j++)
                {

                    GD.Print("subarg " + argType.GenericTypeArguments[j].Name);
                }
            }
            conn.PutVar(args[i]);
            GD.Print("put var");
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
                    //todo revert
                    // var entity = meta.Deserialize(eJson, key);
                    // key.Data.AddEntity(entity, domain.GetType(), key);
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
