using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityVarMeta<TEntity, TProperty> : IEntityVarMeta<TEntity> where TEntity : Entity
{
    public string PropertyName { get; private set; }
    protected Func<TEntity, TProperty> FieldGetter { get; private set; }
    protected Action<TEntity, TProperty> FieldSetter { get; private set; }
    
    public EntityVarMeta(PropertyInfo prop)
    {
        var getMi = prop.GetGetMethod();
        FieldGetter = getMi.MakeInstanceMethodDelegate<Func<TEntity, TProperty>>();
        var setMi = prop.GetSetMethod(true);
        FieldSetter = setMi.MakeInstanceMethodDelegate<Action<TEntity, TProperty>>();
    }

    public virtual void Set(TEntity e, object receivedValue, ServerWriteKey key)
    {
        FieldSetter(e, (TProperty)receivedValue);
    }
    public virtual void Set(TEntity e, object receivedValue, CreateWriteKey key)
    {
        FieldSetter(e, (TProperty)receivedValue);
    }
}