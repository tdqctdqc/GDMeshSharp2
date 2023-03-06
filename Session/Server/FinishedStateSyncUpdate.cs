using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinishedStateSyncUpdate : Update
{
    public Guid PlayerGuid { get; private set; }
    public static FinishedStateSyncUpdate Create(Guid playerGuid, HostWriteKey key)
    {
        return new FinishedStateSyncUpdate(playerGuid);
    }
    public FinishedStateSyncUpdate(Guid playerGuid) : base()
    {
        PlayerGuid = playerGuid;
    }

    public override void Enact(ServerWriteKey key)
    {
        GD.Print("Finished state sync");
        Game.I.SetPlayerGuid(PlayerGuid, key);
        key.Data.Notices.FinishedStateSync?.Invoke();
    }
}