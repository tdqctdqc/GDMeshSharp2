using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer
{
    private Queue<byte[]> _queuedToUnwrap;
    private Queue<Update> _unwrappedUpdates;
    private PacketPeerStream _packetStream;
    public RemoteSyncer(PacketPeerStream packetStream)
    {
        _queuedToUnwrap = new Queue<byte[]>();
        _unwrappedUpdates = new Queue<Update>();
        _packetStream = packetStream;
    }

    public void Process(ServerWriteKey key)
    {
        var availPackets = _packetStream.GetAvailablePacketCount();
        if (availPackets > 0)
        {
            while (_packetStream.GetAvailablePacketCount() > 0)
            {
                var packet = _packetStream.GetPacket();
                ReceivePacket(packet);
            }
            var u = new RequestMoreBytesCommand(_packetStream.InputBufferMaxSize, key);
            _packetStream.PutPacket(u.GetPacketBytes());
        }

        if (_queuedToUnwrap.Count == 0)
        {
            while (_unwrappedUpdates.Count > 0)
            {
                ProcessUpdate(_unwrappedUpdates.Dequeue(), key);
            }
        }
    }

    private void ReceivePacket(byte[] packet)
    {
        _queuedToUnwrap.Enqueue(packet);
    }

    private void ProcessPacket(byte[] packet)
    {
        var update = Update.DecodePacket(packet);
    }

    private void ProcessUpdate(Update update, ServerWriteKey key)
    {
        if (update is RequestAvailableBytesUpdate r)
        {
            var c = new RequestMoreBytesCommand(_packetStream.StreamPeer.GetAvailableBytes(), key);
            _packetStream.PutPacket(c.GetPacketBytes());
        }
        update.Enact(key);
    }
}