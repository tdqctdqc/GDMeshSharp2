
using System;

public class Entity1To1Indexer<TEntity, TKey> : Entity1to1PropIndexer<TEntity, int?>
    where TEntity : Entity where TKey : Entity
{
    public TEntity this[TKey k] => this[k.Id];
    public static Entity1To1Indexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, EntityRef<TKey>> get)
    {
        return new Entity1To1Indexer<TEntity, TKey>(data, get);
    }
    public static Entity1To1Indexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, EntityRef<TKey>> get, string keyFieldName)
    {
        return new Entity1To1Indexer<TEntity, TKey>(data, get, keyFieldName);
    }
    private Entity1To1Indexer(Data data, 
        Func<TEntity, EntityRef<TKey>> get, 
        string keyFieldName) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
        Action<ValChangeNotice<EntityRef<TKey>>> callback = n =>
        {
            if(n.OldVal != null) _dic.Remove(n.OldVal.RefId);
            if(n.NewVal != null) _dic[n.NewVal.RefId] = (TEntity)data[n.Entity.Id];
        };
        var refAction = new RefAction<ValChangeNotice<EntityRef<TKey>>>();
        refAction.Subscribe(callback);
        data.SubscribeForValueChange<TEntity, EntityRef<TKey>>(keyFieldName, refAction);
    }
    private Entity1To1Indexer(Data data, Func<TEntity, EntityRef<TKey>> get) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
    }

    public bool ContainsKey(TKey k)
    {
        return ContainsKey(k.Id);
    }
}
