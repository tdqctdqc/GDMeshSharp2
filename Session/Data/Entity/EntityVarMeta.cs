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
        var getMi = prop.GetGetMethod();
        var getPropDel = getMi.MakeInstanceMethodDelegate<Func<TEntity, TProperty>>();
        GetProperty = e =>
        {
            GD.Print("getting");
            GD.Print(e == null);
            var p = getPropDel(e);
            GD.Print("got");
            return p;
        };
        var setMi = prop.GetSetMethod(true);
        SetProperty = setMi.MakeInstanceMethodDelegate<Action<TEntity, TProperty>>();
    }

    public virtual object GetForSerialize(TEntity e)
    {
        return GetProperty(e);
    }
    public virtual object GetFromSerialized(byte[] bytes)
    {
        return Game.I.Serializer.Deserialize<TProperty>(bytes);
    }
    public virtual void Set(TEntity e, object receivedValue, ServerWriteKey key)
    {
        SetProperty(e, (TProperty)receivedValue);
    }
    public virtual void Set(TEntity e, object receivedValue, CreateWriteKey key)
    {
        SetProperty(e, (TProperty)receivedValue);
    }
}