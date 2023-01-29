using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed class EntityVarUpdate : Update
{
    public string FieldName { get; private set; }
    public int EntityId { get; private set; }
    public object NewVal { get; private set; }
    public static void Send<TValue>(string fieldName, int entityId, TValue newVal, HostWriteKey key)
    {
        var u = new EntityVarUpdate(fieldName, entityId,
            newVal, key);
        key.HostServer.QueueUpdate(u);        
    }
    private EntityVarUpdate(string fieldName, int entityId, object newVal, HostWriteKey key) : base(key)
    {
        FieldName = fieldName;
        EntityId = entityId;
        NewVal = newVal;
    }

    public override void Enact(ServerWriteKey key)
    {
        var entity = key.Data[EntityId];
        var meta = entity.GetMeta();
        meta.UpdateEntityVar(FieldName, entity, key, NewVal);
    }
}
