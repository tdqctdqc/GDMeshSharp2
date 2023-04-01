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
        RefAction[] recalcTriggers, RefAction<Tuple<TEntity, TValue>>[] updateTriggers)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, recalcTriggers, updateTriggers);
    }

    private EntityValueCache(Data data, Func<TEntity, TValue> get) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        Initialize(data);
    }
    private EntityValueCache(Data data, Func<TEntity, TValue> get, RefAction[] recalcTriggers,
        RefAction<Tuple<TEntity, TValue>>[] updateTriggers) : base(data)
    {
        _dic = new Dictionary<TEntity, TValue>();
        _get = get;
        foreach (var trigger in recalcTriggers)
        {
            trigger.Subscribe(() =>
            {
                Initialize(data);
            });
        }
        foreach (var trigger in updateTriggers)
        {
            trigger.Subscribe(v =>
            {
                UpdateEntry(v.Item1, v.Item2);
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

    private void UpdateEntry(TEntity e, TValue newVal)
    {
        _dic[e] = newVal;
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
