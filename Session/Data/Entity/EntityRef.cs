using Godot;
using System;
using System.Collections.Generic;

[EntityVariable]
[EntityRef]
public class EntityRef<TRef> where TRef : Entity
{
    public string Name { get; private set; }
    public int EntityId { get; private set; }
    public int RefId { get; private set; }
    public TRef Entity(Data data) => (TRef) data[RefId];
    public EntityRef(int refId, int entityId, string name)
    {
        Name = name;
        RefId = refId;
        EntityId = entityId;
    }

    public static EntityRef<TRef> Construct(int refId, int entityId, string name)
    {
        return new EntityRef<TRef>(refId, entityId, name);
    }
    public void Update(HostWriteKey key, int newValue, HostServer server)
    {
        RefId = newValue;
        key.Data.EntityRepos[EntityId].RaiseValueChangedNotice(Name, EntityId, key);
        var update = EntityVarUpdate.Encode<int>(Name, EntityId, newValue, key);
        server.QueueUpdate(update);
    }
    public static void ReceiveUpdate(EntityRef<TRef> str, ServerWriteKey key, string newValueJson)
    {
        var list = System.Text.Json.JsonSerializer.Deserialize<List<int>>(newValueJson);

        str.RefId = list[0];
        str.EntityId = list[1];
        key.Data.EntityRepos[str.EntityId].RaiseValueChangedNotice(str.Name, str.EntityId, key);
    }
    public void SetByProcedure(ProcedureWriteKey key, int newRefId)
    {
        RefId = newRefId;
    }
    public static string Serialize(EntityRef<TRef> es)
    {
        return System.Text.Json.JsonSerializer.Serialize<List<int>>( new List<int>{es.EntityId, es.RefId});
    }
    public static EntityRef<TRef> Deserialize(string json, string name, Entity entity)
    {
        var list = System.Text.Json.JsonSerializer.Deserialize<List<int>>(json);
        var hostId = list[0];
        var refId = list[1];
        return Construct(refId, hostId, name);
    }
}
