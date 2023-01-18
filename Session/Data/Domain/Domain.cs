using Godot;
using System;
using System.Collections.Generic;

public abstract class Domain 
{
    public IReadOnlyDictionary<Type, IRepo> Repos => _repos;
    protected Dictionary<Type, IRepo> _repos;

    public Domain()
    {
        _repos = new Dictionary<Type, IRepo>();
    }
    public Repository<T> GetRepo<T>() where T : Entity
    {
        return (Repository<T>)_repos[typeof(T)];
    }

    public IRepo GetRepo(Type entityType)
    {
        return _repos[entityType];
    }
}
