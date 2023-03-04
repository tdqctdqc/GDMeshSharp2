using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public sealed class EntityVarUpdate : Update
{
    public string FieldName { get; private set; }
    public int EntityId { get; private set; }
    public byte[] NewVal { get; private set; }
    public static EntityVarUpdate Create(string fieldName, int entityId, byte[] newVal, HostWriteKey key)
    {
        return new EntityVarUpdate(fieldName, entityId,
            newVal);
    }
    [SerializationConstructor] private EntityVarUpdate(string fieldName, int entityId, byte[] newVal)
    {
        FieldName = fieldName;
        EntityId = entityId;
        NewVal = newVal;
    }

    public override void Enact(ServerWriteKey key)
    {
        var entity = key.Data[EntityId];
        var meta = entity.GetMeta();
        meta.UpdateEntityVarServer(FieldName, entity, key, NewVal);
    }
}
