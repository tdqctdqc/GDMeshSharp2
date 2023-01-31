using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class HostSyncer
{
    private PacketPeerStream _packetStream;
    private Queue<byte[]> _packetsToSend;

    public HostSyncer(PacketPeerStream packetStream)
    {
        _packetStream = packetStream;
    }
    
}