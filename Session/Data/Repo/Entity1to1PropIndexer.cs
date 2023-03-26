
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Entity1to1PropIndexer<TEntity, TKey> : AuxData<TEntity>
    where TEntity : Entity
{
    public TEntity this[TKey e] => _dic.ContainsKey(e) ? _dic[e] : null;
    protected Dictionary<TKey, TEntity> _dic;
    protected Func<TEntity, TKey> _get;
    
    public static Entity1to1PropIndexer<TEntity, TKey> CreateConstant(Data data, Func<TEntity, TKey> get)
    {
        return new Entity1to1PropIndexer<TEntity, TKey>(data, get);
    }
    public static Entity1to1PropIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, TKey> get, string keyFieldName)
    {
        return new Entity1to1PropIndexer<TEntity, TKey>(data, get, keyFieldName);
    }
    public static Entity1to1PropIndexer<TEntity, TKey> CreateTrigger(Data data, Func<TEntity, TKey> get, RefAction trigger)
    {
        return new Entity1to1PropIndexer<TEntity, TKey>(data, get, trigger);
    }
    protected Entity1to1PropIndexer(Data data, Func<TEntity, TKey> get) : base(data)
    {
        _get = get;
        Initialize(data);
    }
    protected Entity1to1PropIndexer(Data data, Func<TEntity, TKey> get, string keyFieldName) : base(data)
    {
        _get = get;
        Action<ValChangeNotice<TKey>> callback = n =>
        {
            if(n.OldVal != null) _dic.Remove(n.OldVal);
            if(n.NewVal != null) _dic[n.NewVal] = (TEntity)data[n.EntityId];
        };
        EntityValChangedHandler<TEntity, TKey>.RegisterForAll(keyFieldName, callback);
        Initialize(data);
    }
    protected Entity1to1PropIndexer(Data data, Func<TEntity, TKey> get, RefAction trigger) : base(data)
    {
        _get = get;
        trigger.Subscribe(() =>
        {
            Initialize(data);
        });
        Initialize(data);
    }
    private void Initialize(Data data)
    {
        _dic = new Dictionary<TKey, TEntity>();
        var register = data.GetRegister<TEntity>();
        foreach (var e in register.Entities)
        {
            _dic.Add(_get((TEntity)e), (TEntity)e);
        }
    }
    public bool ContainsKey(TKey e)
    {
        return _dic.ContainsKey(e);
    }
    public override void HandleAdded(TEntity added)
    {
        var val = _get(added);
        if(val != null)
        {
            _dic[val] = added;
        }
    }

    public override void HandleRemoved(TEntity removing)
    {
        var val = _get(removing);
        if (val != null) _dic.Remove(val);
    }
}
