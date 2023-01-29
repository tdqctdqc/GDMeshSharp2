using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinishedStateTransferUpdate : Update
{
    public FinishedStateTransferUpdate(HostWriteKey key) : base(key)
    {
    }

    public override void Enact(ServerWriteKey key)
    {
        var session = key.Session;
        session.StartClient(key.Server, key);
    }
}