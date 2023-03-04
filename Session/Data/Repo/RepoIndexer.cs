
using System;
using System.Collections.Generic;
using Godot;

public class RepoIndexer<TEntity, TKey> : RepoAuxData<TEntity>
    where TEntity : Entity
{
    public TEntity this[TKey e] => _dic.ContainsKey(e) ? _dic[e] : null;
    private Dictionary<TKey, TEntity> _dic;
    private Func<TEntity, TKey> _get;

    public RepoIndexer(Data data, Func<TEntity, TKey> get) : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TEntity>();
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
