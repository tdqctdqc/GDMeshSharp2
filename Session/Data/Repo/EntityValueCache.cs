using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityValueCache<TEntity, TValue> : AuxData<TEntity>
    where TEntity : Entity
{
    public TValue this[TEntity t] => _dic.ContainsKey(t) ? _dic[t] : default;
    protected Dictionary<TEntity, TValue> _dic;
    protected Func<TEntity, TValue> _get;
    
    public static EntityValueCache<TEntity, TValue> CreateConstant(Data data, Func<TEntity, TValue> get)
    {
        return new EntityValueCache<TEntity, TValue>(data, get);
    }
    public static EntityValueCache<TEntity, TValue> CreateTrigger(Data data, Func<TEntity, TValue> get, 
        EntityRegister<TEntity> repo, params RefAction[] triggers)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, triggers);
    }

    private EntityValueCache(Data data, Func<TEntity, TValue> get) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        Initialize(data);
    }
    private EntityValueCache(Data data, Func<TEntity, TValue> get, params RefAction[] triggers) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        foreach (var trigger in triggers)
        {
            trigger.Subscribe(() =>
            {
                Initialize(data);
            });
        }
        
        Initialize(data);
    }
    private void Initialize(Data data)
    {
        var register = data.GetRegister<TEntity>();
        _dic.Clear();
        
        foreach (var e in register.Entities)
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
