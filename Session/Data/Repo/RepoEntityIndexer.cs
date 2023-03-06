
using System;

public class RepoEntityIndexer<TEntity, TKey> : RepoIndexer<TEntity, TKey>
    where TEntity : Entity where TKey : Entity
{
    //todo make so can reference entity that is in the middle of being added
    public static RepoEntityIndexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, TKey> get)
    {
        return new RepoEntityIndexer<TEntity, TKey>(data, get);
    }
    public static RepoEntityIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, TKey> get, string keyFieldName)
    {
        return new RepoEntityIndexer<TEntity, TKey>(data, get, keyFieldName);
    }
    private RepoEntityIndexer(Data data, Func<TEntity, TKey> get, string keyFieldName) 
        : base(data, e => get(e))
    {
        Action<ValueChangedNotice<TEntity, TKey>> callback = n =>
        {
            if(n.OldVal != null) _dic.Remove(n.OldVal);
            if(n.NewVal != null) _dic[n.NewVal] = n.Entity;
        };
        ValueChangedHandler<TEntity, TKey>.RegisterForAll(keyFieldName, callback);
    }
    private RepoEntityIndexer(Data data, Func<TEntity, TKey> get) 
        : base(data, e => get(e))
    {
    }
}
