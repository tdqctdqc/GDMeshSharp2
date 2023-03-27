using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityRegister<TEntity> : IEntityRegister
    where TEntity : Entity
{
    public Type EntityType => typeof(TEntity);
    private Data _data;
    IReadOnlyCollection<Entity> IEntityRegister.Entities => Entities; 
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
        data.RegisterForCreation<TEntity>(n => Add(n));
        data.RegisterForDestruction<TEntity>(n => Remove(n));
    }

    private void Add(EntityCreatedNotice n)
    {
        _entities.Add((TEntity)n.Entity);
    }

    private void Remove(EntityDestroyedNotice n)
    {
        _entities.Remove((TEntity)n.Entity);
    }
}
