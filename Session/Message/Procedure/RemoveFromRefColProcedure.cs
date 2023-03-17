
using System.Collections.Generic;
using MessagePack;

public class RemoveFromRefColProcedure : Procedure
{
    public EntityRef<Entity> Entity { get; private set; }
    public string CollectionName { get; private set; }
    public List<int> ToRemove { get; private set; }

    public static RemoveFromRefColProcedure Create(Entity e, string colName, List<int> toAdd)
    {
        return new RemoveFromRefColProcedure(e.MakeRef(), colName, toAdd);
    }
    [SerializationConstructor] private RemoveFromRefColProcedure(EntityRef<Entity> entity, string collectionName, List<int> toRemove)
    {
        Entity = entity;
        CollectionName = collectionName;
        ToRemove = toRemove;
    }

    public override bool Valid(Data data)
    {
        return Entity.CheckExists(data);
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var e = Entity.Entity();
        var meta = e.GetMeta();
        var col = meta.GetRefCollection(CollectionName, e, key);
        col.RemoveByProcedure(ToRemove, key);
    }
}
