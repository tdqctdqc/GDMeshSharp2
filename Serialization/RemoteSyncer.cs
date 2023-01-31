using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer
{
    private PacketPeerStream _packetStream;
    public RemoteSyncer(PacketPeerStream packetStream)
    {
        _packetStream = packetStream;
    }
}