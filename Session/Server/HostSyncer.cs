using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class HostSyncer : Syncer
{
    private Queue<byte[]> _peerQueue;
    public HostSyncer(PacketPeerStream packetStream, MessageManager msg) : base(packetStream, msg)
    {
        _peerQueue = new Queue<byte[]>();
    }

    public void Sync(HostWriteKey key)
    {
        GD.Print("Syncing");
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

        var done = FinishedStateSyncUpdate.Create(key);
        var bytes2 = _msg.WrapUpdate(done);
        QueuePacket(bytes2);
        PushPackets(key);
    }

    public void QueuePacket(byte[] packet)
    {
        _peerQueue.Enqueue(packet);
    }

    private int _packetsSent = 0;
    public void PushPackets(HostWriteKey key)
    {
        bool push = true;
        while (push && _peerQueue.Count > 0)
        {
            _packetsSent++;
            PushPacket(_peerQueue.Dequeue());
        }
        GD.Print(_packetsSent);
    }
}