
using System;
using System.Collections.Generic;
using Godot;

public class RepoAuxLookup<TEntity, TKey> : RepoAuxData<TEntity>
    where TEntity : Entity
{
    public TEntity this[TKey e] => _dic[e];
    private Dictionary<TKey, TEntity> _dic;
    private Func<TEntity, TKey> _get;

    public RepoAuxLookup(Data data, Func<TEntity, TKey> get,
        out Action<TEntity> updateTrigger) : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TEntity>();
        updateTrigger = HandleAdded;
    }

    public bool ContainsKey(TKey e)
    {
        return _dic.ContainsKey(e);
    }
    public override void HandleAdded(TEntity added)
    {
        _dic[_get(added)] = added;
    }

    public override void HandleRemoved(TEntity removing)
    {
        _dic.Remove(_get(removing));
    }
}
