using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class HostSyncer : Syncer
{
    private Queue<byte[]> _peerQueue;
    public HostSyncer(PacketPeerStream packetStream, HostLogic logic, Guid fromGuid) 
        : base(packetStream, 
            new MessageManager(u => { }, 
                p => { }, 
                c =>
                {
                    c.SetGuid(fromGuid);
                    logic.CommandQueue.Enqueue(c);
                },
                d => { }),
            fromGuid)
    {
        _peerQueue = new Queue<byte[]>();
    }

    public void Sync(Guid newPlayerGuid, HostWriteKey key)
    {
        GD.Print("Syncing");
        Player.Create(key.IdDispenser.GetID(), newPlayerGuid, "doot", key);

        var data = key.Data;
        foreach (var kvp1 in data.Domains)
        {
            var dom = kvp1.Value;
            foreach (var kvp2 in dom.Repos)
            {
                var repo = kvp2.Value;
                GD.Print(kvp2.Key.Name);
                foreach (var e in repo.Entities)
                {
                    var u = EntityCreationUpdate.Create(e, key);
                    var bytes = _msg.WrapUpdate(u);
                    QueuePacket(bytes);
                }
            }
        }

        
        var done = FinishedStateSyncUpdate.Create(newPlayerGuid, key);
        var bytes2 = _msg.WrapUpdate(done);
        QueuePacket(bytes2);
        PushPackets(key);
    }

    public void QueuePacket(byte[] packet)
    {
        _peerQueue.Enqueue(packet);
    }

    public void PushPackets(HostWriteKey key)
    {
        bool push = true;
        while (push && _peerQueue.Count > 0)
        {
            PushPacket(_peerQueue.Dequeue());
        }
    }
}