using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class HostSyncer
{
    private PacketPeerStream _packetStream;
    private int _availableRemoteBytes;
    private Queue<byte[]> _packetsToSend;

    public HostSyncer(PacketPeerStream packetStream)
    {
        _packetStream = packetStream;
        _packetsToSend = new Queue<byte[]>();
        _availableRemoteBytes = 0;
    }

    public void Process()
    {
        while(_packetStream.GetAvailablePacketCount() > 0)
        {
            var packetFromRemote = _packetStream.GetPacket();
            var command = Command.DecodePacket(packetFromRemote);
            HandleCommandFromRemote(command);
        }
    }
    public void QueueUpdate(Update update)
    {
        var packet = update.GetPacketBytes();
        _packetsToSend.Enqueue(packet);
    }

    public void DoStateTransfer(HostWriteKey key)
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
                    _packetsToSend.Enqueue(wrapperBytes);
                }
            }
        }
        var done = new FinishedStateTransferUpdate(key);
        var doneBytes = done.GetPacketBytes();
        _packetsToSend.Enqueue(doneBytes);

        var req = new RequestAvailableBytesUpdate(key);
        _packetStream.PutPacket(req.GetPacketBytes());
    }
    private void HandleCommandFromRemote(Command c)
    {
        if (c is RequestMoreBytesCommand r)
        {
            _availableRemoteBytes = r.AvailableBytes;
            SendPackets();
        }
    }

    private void SendPackets()
    {
        var bytesSent = 0;
        while (_packetsToSend.Count > 0)
        {
            var packet = _packetsToSend.Peek();
            if (packet.Length + bytesSent < _availableRemoteBytes * .8f)
            {
                _packetsToSend.Dequeue();
                bytesSent += packet.Length;
                var err = _packetStream.PutPacket(packet);
                if (err != Error.Ok) throw new Exception();
            }
        }

        _availableRemoteBytes -= bytesSent;
    }
}