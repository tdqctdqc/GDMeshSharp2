using Godot;
using System;

public class EntityDeletionUpdate : IUpdate
{
    string IUpdate.UpdateType => UpdateType;
    public static string UpdateType => "EntityDeletion";
    public int EntityId { get; private set; }

    public EntityDeletionUpdate(int entityId)
    {
        EntityId = entityId;
    }

    public string Serialize()
    {
        return EntityId.ToString();
    }

    public static void DeserializeAndEnact(string json, ServerWriteKey key)
    {
        Game.I.Session.Data.RemoveEntity(Game.I.Session.Data[json.ToInt()], key);
    }
}
