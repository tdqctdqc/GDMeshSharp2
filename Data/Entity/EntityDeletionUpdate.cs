using Godot;
using System;

public class EntityDeletionUpdate : Update
{
    public int EntityId { get; private set; }

    public static void Send(int entityId, HostWriteKey key)
    {
        var u = new EntityDeletionUpdate(entityId, key);
        key.HostServer.QueueUpdate(u);
    }
    public EntityDeletionUpdate(int entityId, HostWriteKey key) : base(key)
    {
        EntityId = entityId;
    }

    public override void Enact(ServerWriteKey key)
    {
        var e = key.Data.Entities[EntityId];
        var meta = e.GetMeta();
        meta.RemoveFromDataFromUpdate(e, key);
    }
}