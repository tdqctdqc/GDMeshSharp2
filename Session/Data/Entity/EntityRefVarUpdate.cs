using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityRefVarUpdate : Update
{
    public string FieldName { get; private set; }
    public int EntityId { get; private set; }
    public object NewVal { get; private set; }
    public static void Send<TValue>(string fieldName, int entityId, TValue newVal, 
        HostWriteKey key)
    {
        var update = new EntityRefVarUpdate(fieldName, entityId,
            newVal, key);
        key.HostServer.QueueUpdate(update);
    }
    private EntityRefVarUpdate(string fieldName, int entityId, 
        object newVal, HostWriteKey key) 
        : base(key)
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

    private static EntityRefVarUpdate DeserializeConstructor(object[] args)
    {
        return new EntityRefVarUpdate(args);
    }
    private EntityRefVarUpdate(object[] args) : base(args) {}
}