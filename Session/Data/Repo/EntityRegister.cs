using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityRegister<TEntity> : IEntityRegister
    where TEntity : Entity
{
    public Type EntityType => typeof(TEntity);
    private Data _data;
    public ReadOnlyHash<TEntity> Entities { get; private set; }
    private HashSet<TEntity> _entities;
    public TEntity this[int id] => (TEntity)_data[id];

    public static IEntityRegister ConstructFromType(Type type, Data data)
    {
        if (typeof(Entity).IsAssignableFrom(type) == false) throw new Exception();
        var construct = typeof(EntityRegister<TEntity>).GetMethod(nameof(ConstructFromType),
            BindingFlags.Static | BindingFlags.NonPublic);
        var generic = construct.MakeGenericMethod(type);
        return (IEntityRegister) generic.Invoke(null, new object[] {data});
    }
    private static EntityRegister<T> ConstructFromType<T>(Data data) where T : Entity
    {
        return new EntityRegister<T>(data);
    }
    public EntityRegister(Data data)
    {
        _data = data;
        _entities = new HashSet<TEntity>();
        Entities = new ReadOnlyHash<TEntity>(_entities);
        
        EntityCreatedHandler<TEntity>.Register(Add);
        EntityDestroyedHandler<TEntity>.Register(Remove);
    }

    private void Add(EntityCreatedNotice<TEntity> n)
    {
        _entities.Add(n.Entity);
    }

    private void Remove(EntityDestroyedNotice<TEntity> n)
    {
        _entities.Remove(n.Entity);
    }
}
