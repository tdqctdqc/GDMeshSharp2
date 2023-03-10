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
    Type IRepo.EntityType => typeof(T);
    
    public Repository(Domain domain, Data data)
    {
        Domain = domain;
        _entityValueUpdatedActions = new Dictionary<string, object>();
        _entitiesById = new Dictionary<int, T>();
        _entities = new HashSet<T>();
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

    public bool Contains(T t)
    {
        return _entities.Contains(t);
    }

}
