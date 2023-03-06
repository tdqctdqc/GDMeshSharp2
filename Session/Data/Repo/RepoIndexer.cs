
using System;
using System.Collections.Generic;
using Godot;

public class RepoIndexer<TEntity, TKey> : RepoAuxData<TEntity>
    where TEntity : Entity
{
    public TEntity this[TKey e] => _dic.ContainsKey(e) ? _dic[e] : null;
    protected Dictionary<TKey, TEntity> _dic;
    protected Func<TEntity, TKey> _get;
    
    public static RepoIndexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, TKey> get)
    {
        return new RepoIndexer<TEntity, TKey>(data, get);
    }
    public static RepoIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, TKey> get, string keyFieldName)
    {
        return new RepoIndexer<TEntity, TKey>(data, get, keyFieldName);
    }
    protected RepoIndexer(Data data, Func<TEntity, TKey> get) : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TEntity>();
    }
    private RepoIndexer(Data data, Func<TEntity, TKey> get, string keyFieldName) : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TEntity>();
        Action<ValueChangedNotice<TEntity, TKey>> callback = n =>
        {
            if(n.OldVal != null) _dic.Remove(n.OldVal);
            if(n.NewVal != null) _dic[n.NewVal] = n.Entity;
        };
        ValueChangedHandler<TEntity, TKey>.RegisterForAll(keyFieldName, callback);
    }

    public bool ContainsKey(TKey e)
    {
        return _dic.ContainsKey(e);
    }
    public override void HandleAdded(TEntity added)
    {
        var val = _get(added);
        if(val != null) _dic[val] = added;
    }

    public override void HandleRemoved(TEntity removing)
    {
        var val = _get(removing);
        if (val != null) _dic.Remove(val);
    }
}
