using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RequestAvailableBytesUpdate : Update
{
    public RequestAvailableBytesUpdate(HostWriteKey key) : base(key)
    {
    }

    public override void Enact(ServerWriteKey key)
    {
        
    }
}