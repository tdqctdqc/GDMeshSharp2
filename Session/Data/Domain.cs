using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class Domain 
{
    public Dictionary<Type, IEntityRegister> Registers { get; private set; }
    public IReadOnlyDictionary<Type, IRepo> Repos => _repos;
    protected Dictionary<Type, IRepo> _repos;
    public Data Data { get; private set; }
    public Domain(Data data, Type domainType)
    {
        Data = data;
        Registers = new Dictionary<Type, IEntityRegister>();
        _repos = new Dictionary<Type, IRepo>();
        var s = Game.I.Serializer;
        var entityTypes = s.ConcreteEntityTypes
            .Where(t => s.GetEntityMeta(t).DomainType == domainType);
        foreach (var entityType in entityTypes)
        {
            GD.Print("adding register for " + entityType.Name);
            AddRegister(entityType);
        }
    }
    public Repository<T> GetRepo<T>() where T : Entity
    {
        return (Repository<T>)_repos[typeof(T)];
    }
    public EntityRegister<T> GetRegister<T>() where T : Entity
    {
        return (EntityRegister<T>)Registers[typeof(T)];
    }
    private void AddRegister(Type entityType)
    {
        Registers.Add(entityType, EntityRegister<Entity>.ConstructFromType(entityType, Data));
    }
    public IRepo GetRepo(Type entityType)
    {
        return _repos[entityType];
    }

    protected void AddRepo<T>(Repository<T> repo) where T : Entity
    {
        _repos.Add(typeof(T), repo);
    }

}
