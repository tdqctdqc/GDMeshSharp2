using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class Domain 
{
    public Dictionary<Type, IEntityRegister> Registers { get; private set; }
    public IReadOnlyDictionary<Type, IAux> Repos => _repos;
    protected Dictionary<Type, IAux> _repos;
    public Data Data { get; private set; }
    public Domain(Type domainType)
    {
        Registers = new Dictionary<Type, IEntityRegister>();
        _repos = new Dictionary<Type, IAux>();
        var s = Game.I.Serializer;
        var entityTypes = s.ConcreteEntityTypes
            .Where(t => s.GetEntityMeta(t).DomainType == domainType);
        foreach (var entityType in entityTypes)
        {
            AddRegister(entityType);
        }
    }

    public void Setup(Data data)
    {
        Data = data;
        Setup();
    }

    protected abstract void Setup();
    
    public EntityAux<T> GetRepo<T>() where T : Entity
    {
        return (EntityAux<T>)_repos[typeof(T)];
    }
    public EntityRegister<T> GetRegister<T>() where T : Entity
    {
        return (EntityRegister<T>)Registers[typeof(T)];
    }
    private void AddRegister(Type entityType)
    {
        Registers.Add(entityType, EntityRegister<Entity>.ConstructFromType(entityType, Data));
    }
    public IAux GetRepo(Type entityType)
    {
        return _repos[entityType];
    }

    protected void AddRepo<T>(EntityAux<T> repo) where T : Entity
    {
        _repos.Add(typeof(T), repo);
    }

}
