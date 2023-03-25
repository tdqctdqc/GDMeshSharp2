
using System;

public class Entity1To1EntityPropIndexer<TEntity, TKey> : Entity1to1PropIndexer<TEntity, int?>
    where TEntity : Entity where TKey : Entity
{
    public TEntity this[TKey k] => this[k.Id];
    public static Entity1To1EntityPropIndexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, EntityRef<TKey>> get)
    {
        return new Entity1To1EntityPropIndexer<TEntity, TKey>(data, get);
    }
    public static Entity1To1EntityPropIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, EntityRef<TKey>> get, string keyFieldName)
    {
        return new Entity1To1EntityPropIndexer<TEntity, TKey>(data, get, keyFieldName);
    }
    private Entity1To1EntityPropIndexer(Data data, 
        Func<TEntity, EntityRef<TKey>> get, 
        string keyFieldName) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
        Action<ValueChangedNotice<TEntity, TKey>> callback = n =>
        {
            if(n.OldVal != null) _dic.Remove(n.OldVal.Id);
            if(n.NewVal != null) _dic[n.NewVal.Id] = n.Entity;
        };
        EntityValChangedHandler<TEntity, TKey>.RegisterForAll(keyFieldName, callback);
    }
    private Entity1To1EntityPropIndexer(Data data, Func<TEntity, EntityRef<TKey>> get) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
    }

    public bool ContainsKey(TKey k)
    {
        return ContainsKey(k.Id);
    }
}
