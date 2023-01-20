using Godot;
using System;
using System.Collections.Generic;

public class Repository<T> : IRepo where T : Entity
{
    public Domain Domain { get; private set; }
    public Action<T, WriteKey> AddedEntity { get; set; }
    public Action<T, WriteKey> RemovingEntity { get; set; }
    private Dictionary<string, Action<int, WriteKey>> _entityValueUpdatedActions;
    public T this[int id] => _entitiesById[id];
    protected Dictionary<int, T> _entitiesById;
    IReadOnlyList<Entity> IRepo.Entities => _entities;
    public IReadOnlyList<T> Entities => _entities;
    private List<T> _entities;
    private ClientWriteKey _weakKey;
    
    public Repository(Domain domain, Data data)
    {
        Domain = domain;
        _entityValueUpdatedActions = new Dictionary<string, Action<int, WriteKey>>();
        _entitiesById = new Dictionary<int, T>();
        _entities = new List<T>();
        _weakKey = new ClientWriteKey(data);
    }

    
    public void AddEntity(Entity e, StrongWriteKey key)
    {
        if (e is T t == false) throw new Exception();
        _entitiesById.Add(t.Id, t);
        _entities.Add(t);
        if(key is HostWriteKey)
        {
            AddedEntity?.Invoke(t, key);
        }
        else
        {
            AddedEntity?.Invoke(t, _weakKey);
        }
    }
    public void RemoveEntity(Entity e, StrongWriteKey key)
    {
        if (e is T t == false) throw new Exception();
        if(key is HostWriteKey)
        {
            RemovingEntity?.Invoke(t, key);
        }
        else
        {
            RemovingEntity?.Invoke(t, _weakKey);
        }
        _entitiesById.Remove(t.Id);
        _entities.Remove(t);
    }

    protected void RegisterForValueChangeCallback(string valueName, Action<int, WriteKey> callback) 
    {
        if (_entityValueUpdatedActions.ContainsKey(valueName) == false)
        {
            _entityValueUpdatedActions.Add(valueName, (id, key) => { }); 
        }
        _entityValueUpdatedActions[valueName] += callback;
    }

    public void RaiseValueChangedNotice(string valueName, int id, WriteKey key)
    {
        if (_entitiesById.ContainsKey(id) == false) throw new Exception();
        if(_entityValueUpdatedActions.ContainsKey(valueName))
            _entityValueUpdatedActions[valueName].Invoke(id, key);
    }
}
