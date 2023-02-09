using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class PeerRequest
{
    public PeerRequest(WriteKey key)
    {
        
    }

    public abstract void Respond(Func<Update, byte[]> wrap, PacketProtocol protocol);
}