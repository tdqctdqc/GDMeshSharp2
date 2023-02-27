
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
        _dic[_get(added)] = added;
    }

    public override void HandleRemoved(TEntity removing)
    {
        _dic.Remove(_get(removing));
    }
}
