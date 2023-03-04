using Godot;
using System;
using System.Collections.Generic;

public class Repository<T> : IRepo where T : Entity
{
    public Domain Domain { get; private set; }
    private Dictionary<string, object> _entityValueUpdatedActions;
    public T this[int id] => _entitiesById[id];
    protected Dictionary<int, T> _entitiesById;
    IReadOnlyCollection<Entity> IRepo.Entities => _entities;
    public IReadOnlyCollection<T> Entities => _entities;
    private HashSet<T> _entities;
    protected ClientWriteKey _weakKey;
    Type IRepo.EntityType => typeof(T);
    
    public Repository(Domain domain, Data data)
    {
        Domain = domain;
        _entityValueUpdatedActions = new Dictionary<string, object>();
        _entitiesById = new Dictionary<int, T>();
        _entities = new HashSet<T>();
        _weakKey = new ClientWriteKey(data);
    }
    
    public void AddEntity(Entity e, StrongWriteKey key)
    {
        if (e is T t == false) throw new Exception();
        _entitiesById.Add(t.Id, t);
        _entities.Add(t);
    }
    public void RemoveEntity(Entity e, StrongWriteKey key)
    {
        if (e is T t == false) throw new Exception();
        
        _entitiesById.Remove(t.Id);
        _entities.Remove(t);
    }

    public void RegisterForValueChangeCallback<TProperty>
        (string valueName, Action<ValueChangedNotice<T, TProperty>> callback) 
    {
        if (_entityValueUpdatedActions.ContainsKey(valueName) == false)
        {
            Action<ValueChangedNotice<T, TProperty>> sendNotice = n => { };
            _entityValueUpdatedActions.Add(valueName, sendNotice); 
        }
        var notice = (Action<ValueChangedNotice<T, TProperty>>)_entityValueUpdatedActions[valueName];
        notice += callback;
    }
    public void RaiseValueChangedNotice<TProperty>(string valueName, T t, 
        TProperty oldVal, TProperty newVal,
        WriteKey key)
    {
        if (_entities.Contains(t) == false) throw new Exception();
        if (_entityValueUpdatedActions.ContainsKey(valueName))
        {
            var sendNotice = (Action<ValueChangedNotice<T, TProperty>>)_entityValueUpdatedActions[valueName];
            sendNotice(new ValueChangedNotice<T, TProperty>(t, newVal, oldVal));
        }
    }
}
