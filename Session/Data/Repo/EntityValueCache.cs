using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityValueCache<TEntity, TValue> 
    : AuxData<TEntity>
    where TEntity : Entity
{
    public TValue this[TEntity t] => _dic.ContainsKey(t) ? _dic[t] : default;
    protected Dictionary<TEntity, TValue> _dic;
    protected Func<TEntity, TValue> _get;
    
    public static EntityValueCache<TEntity, TValue> CreateConstant(Data data, Func<TEntity, TValue> get, 
        EntityRegister<TEntity> repo)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, repo);
    }
    public static EntityValueCache<TEntity, TValue> CreateTrigger(Data data, Func<TEntity, TValue> get, 
        EntityRegister<TEntity> repo, params RefAction[] triggers)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, repo, triggers);
    }

    private EntityValueCache(Data data, Func<TEntity, TValue> get, EntityRegister<TEntity> repo) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        Initialize(repo, data);
    }
    private EntityValueCache(Data data, Func<TEntity, TValue> get, EntityRegister<TEntity> repo, 
        params RefAction[] triggers) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        foreach (var trigger in triggers)
        {
            trigger.Subscribe(() =>
            {
                Initialize(repo, data);
            });
        }
        
        Initialize(repo, data);
    }
    private void Initialize(EntityRegister<TEntity> repo, Data data)
    {
        _dic.Clear();
        
        foreach (var e in repo.Entities)
        {
            if (_dic.ContainsKey((TEntity) e))
            {
                //todo fix this duplicate
                continue;
            }
            var v = _get((TEntity) e);
            if (v != null)
            {
                
                _dic.Add((TEntity)e, v);
            }
        }
    }
    public override void HandleAdded(TEntity added)
    {
        _dic[added] = _get(added);
    }

    public override void HandleRemoved(TEntity removing)
    {
        _dic.Remove(removing);
    }
}
