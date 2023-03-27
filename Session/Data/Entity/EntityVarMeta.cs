using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityVarMeta<TEntity, TProperty> : IEntityVarMeta<TEntity> where TEntity : Entity
{
    public string PropertyName { get; private set; }
    protected Func<TEntity, TProperty> GetProperty { get; private set; }
    protected Action<TEntity, TProperty> SetProperty { get; private set; }
    
    public EntityVarMeta(PropertyInfo prop)
    {
        PropertyName = prop.Name;
        var getMi = prop.GetGetMethod();
        if (getMi == null) throw new SerializationException($"No get method for {PropertyName}");
        GetProperty = getMi.MakeInstanceMethodDelegate<Func<TEntity, TProperty>>();
        
        var setMi = prop.GetSetMethod(true);
        if (setMi == null) throw new SerializationException($"No set method for {PropertyName}");
        SetProperty = setMi.MakeInstanceMethodDelegate<Action<TEntity, TProperty>>();
    }

    public virtual object GetForSerialize(TEntity e)
    {
        return GetProperty(e);
    }
    public virtual void Set(TEntity e, object receivedValue, ServerWriteKey key)
    {
        SetProperty(e, (TProperty)receivedValue);
    }
    public virtual void Set(TEntity e, object receivedValue, StrongWriteKey key)
    {
        SetProperty(e, (TProperty)receivedValue);
    }

    public bool Test(TEntity t)
    {
        var prop = GetProperty(t);
        try
        {
            var bytes = Game.I.Serializer.MP.Serialize(prop);
            var deserialized = Game.I.Serializer.MP.Deserialize<TProperty>(bytes);
        }
        catch (Exception e)
        {
            GD.Print($"Couldn't serialize property {PropertyName} for {typeof(TEntity)}");
            throw;
        }

        return true;
    }
}