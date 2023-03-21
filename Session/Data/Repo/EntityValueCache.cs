using System;
using System.Collections.Generic;
using System.Linq;

public class EntityValueCache<TEntity, TValue> 
    : RepoAuxData<TEntity>
    where TEntity : Entity
{
    protected Dictionary<TEntity, TValue> _dic;
    protected Func<TEntity, TValue> _get;
    
    public static EntityValueCache<TEntity, TValue> CreateConstant(Data data, Func<TEntity, TValue> get)
    {
        return new EntityValueCache<TEntity, TValue>(data, get);
    }
    public static EntityValueCache<TEntity, TValue> CreateDynamic(Data data, Func<TEntity, TValue> get, string keyFieldName)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, keyFieldName);
    }
    public static EntityValueCache<TEntity, TValue> CreateTrigger(Data data, Func<TEntity, TValue> get, RefAction trigger)
    {
        return new EntityValueCache<TEntity, TValue>(data, get, trigger);
    }

    private EntityValueCache(Data data, Func<TEntity, TValue> get) : base(data)
    {
        _get = get;
        Initialize(data);
    }
    private EntityValueCache(Data data, Func<TEntity, TValue> get, string keyFieldName) : base(data)
    {
        _get = get;
        Action<ValueChangedNotice<TEntity, TValue>> callback = n =>
        {
            _dic[n.Entity] = n.NewVal;
        };
        ValueChangedHandler<TEntity, TValue>.RegisterForAll(keyFieldName, callback);
        Initialize(data);
    }
    private EntityValueCache(Data data, Func<TEntity, TValue> get, RefAction trigger) : base(data)
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
        _dic = new Dictionary<TEntity, TValue>();
        foreach (var repo in data.EntityRepos.Values.Where(v => v.EntityType == typeof(TEntity)))
        {
            foreach (var e in repo.Entities)
            {
                _dic.Add((TEntity)e, _get((TEntity)e));
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
