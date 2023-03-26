using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class Domain 
{
    public Dictionary<Type, IEntityRegister> Registers { get; private set; }
    public IReadOnlyHash<Type> EntityTypes { get; private set; }
    public Data Data { get; private set; }
    public Domain(Type domainType)
    {
        Registers = new Dictionary<Type, IEntityRegister>();
        var s = Game.I.Serializer;
        var entityTypes = s.ConcreteEntityTypes
            .Where(t => s.GetEntityMeta(t).DomainType == domainType);
        EntityTypes = new ReadOnlyHash<Type>(new HashSet<Type>(entityTypes));
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
    
    public EntityRegister<T> GetRegister<T>() where T : Entity
    {
        return (EntityRegister<T>)Registers[typeof(T)];
    }
    private void AddRegister(Type entityType)
    {
        Registers.Add(entityType, EntityRegister<Entity>.ConstructFromType(entityType, Data));
    }
}
