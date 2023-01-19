using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed class EntityVarUpdate : IUpdate
{
    string IUpdate.UpdateType => UpdateType;
    public static string UpdateType = "Var";
    public string FieldName { get; private set; }
    public int EntityId { get; private set; }
    public string NewVal { get; private set; }
    public static EntityVarUpdate Encode<TValue>(string fieldName, int entityId, TValue newVal, HostWriteKey key)
    {
        return new EntityVarUpdate(fieldName, entityId,
            System.Text.Json.JsonSerializer.Serialize<TValue>(newVal));
    }
    private EntityVarUpdate(string fieldName, int entityId, string newVal)
    {
        FieldName = fieldName;
        EntityId = entityId;
        NewVal = newVal;
    }

    public string Serialize()
    {
        var jsonArray = new System.Text.Json.Nodes.JsonArray();
        jsonArray.Add(FieldName);
        jsonArray.Add(EntityId.ToString());
        jsonArray.Add(NewVal);
        return jsonArray.ToJsonString();
    }
    public static void DeserializeAndEnact(string json, ServerWriteKey key)
    {
        var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
        var update = new EntityVarUpdate(list[0], list[1].ToInt(), list[2]);
        var entity = key.Data[update.EntityId];
        var meta = entity.GetMeta();
        meta.UpdateEntityVar(update.FieldName, entity, key, update.NewVal);
    }
}
