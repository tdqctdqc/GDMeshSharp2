using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinishedStateSyncUpdate : Update
{
    public static FinishedStateSyncUpdate Create(HostWriteKey key)
    {
        return new FinishedStateSyncUpdate();
    }
    public FinishedStateSyncUpdate() : base()
    {
    }

    public override void Enact(ServerWriteKey key)
    {
        GD.Print("Finished state sync");
        key.Data.Notices.FinishedStateSync?.Invoke();
    }
}