
using System;

public class RepoEntityIndexer<TEntity, TKey> : RepoIndexer<TEntity, int?>
    where TEntity : Entity where TKey : Entity
{
    public TEntity this[TKey k] => this[k.Id];
    public static RepoEntityIndexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, EntityRef<TKey>> get)
    {
        return new RepoEntityIndexer<TEntity, TKey>(data, get);
    }
    public static RepoEntityIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, EntityRef<TKey>> get, string keyFieldName)
    {
        return new RepoEntityIndexer<TEntity, TKey>(data, get, keyFieldName);
    }
    private RepoEntityIndexer(Data data, 
        Func<TEntity, EntityRef<TKey>> get, 
        string keyFieldName) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
        Action<ValueChangedNotice<TEntity, TKey>> callback = n =>
        {
            if(n.OldVal != null) _dic.Remove(n.OldVal.Id);
            if(n.NewVal != null) _dic[n.NewVal.Id] = n.Entity;
        };
        ValueChangedHandler<TEntity, TKey>.RegisterForAll(keyFieldName, callback);
    }
    private RepoEntityIndexer(Data data, Func<TEntity, EntityRef<TKey>> get) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
    }

    public bool ContainsKey(TKey k)
    {
        return ContainsKey(k.Id);
    }
}
