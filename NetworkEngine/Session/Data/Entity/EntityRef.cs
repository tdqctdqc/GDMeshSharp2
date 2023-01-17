using Godot;
using System;

[EntityVariable]
[EntityRef]
public class EntityRef<TRef> where TRef : Entity
{
    public string Name { get; private set; }
    public int HostId { get; private set; }
    public int RefId { get; private set; }
    public TRef ReferedEntity => (TRef) Game.I.Session.Data[RefId];
    public EntityRef(int refId, int hostId, string name)
    {
        //only want to call this for deserialization and from construct
        Name = name;
        RefId = refId;
        HostId = hostId;
    }

    public static EntityRef<TRef> Construct(TRef refer, Entity host, string name)
    {
        return new EntityRef<TRef>(refer.Id.Value, host.Id.Value, name);
    }
    public void Update(HostWriteKey key, int newValue, HostServer server)
    {
        RefId = newValue;
        Game.I.Session.Data.EntityRepos[HostId].RaiseValueChangedNotice(Name, HostId, key);
        var update = EntityVarUpdate.Encode<int>(Name, HostId, newValue, key);
        server.QueueUpdate(update);
    }
    public static void ReceiveUpdate(EntityRef<TRef> str, ServerWriteKey key, string newValueJson)
    {
        str.RefId = System.Text.Json.JsonSerializer.Deserialize<int>(newValueJson);
        Game.I.Session.Data.EntityRepos[str.HostId].RaiseValueChangedNotice(str.Name, str.HostId, key);
    }
    public void SetByProcedure(ProcedureWriteKey key, int newRefId)
    {
        RefId = newRefId;
    }
    public static string Serialize(EntityRef<TRef> es)
    {
        return System.Text.Json.JsonSerializer.Serialize<int>(es.RefId);
    }
    public static EntityRef<TRef> Deserialize(string json, string name, Entity entity)
    {
        var refId = System.Text.Json.JsonSerializer.Deserialize<int>(json);
        var referedEntity = (TRef) Game.I.Session.Data[refId];
        return Construct(referedEntity, entity, name);
    }
}
