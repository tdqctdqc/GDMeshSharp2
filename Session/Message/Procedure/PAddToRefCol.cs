
using System.Collections.Generic;
using MessagePack;

public class PAddToRefCol : Procedure
{
    public EntityRef<Entity> Entity { get; private set; }
    public string CollectionName { get; private set; }
    public List<int> ToAdd { get; private set; }

    public static PAddToRefCol Create(Entity e, string colName, List<int> toAdd)
    {
        return new PAddToRefCol(e.MakeRef(), colName, toAdd);
    }
    [SerializationConstructor] private PAddToRefCol(EntityRef<Entity> entity, string collectionName, List<int> toAdd)
    {
        Entity = entity;
        CollectionName = collectionName;
        ToAdd = toAdd;
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
        col.AddByProcedure(ToAdd, key);
    }
}
