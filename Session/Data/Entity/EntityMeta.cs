using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

public class EntityMeta<T> : IEntityMeta where T : Entity
{
    public IReadOnlyList<string> FieldNames => _fieldNames;
    private List<string> _fieldNames;
    public Dictionary<string, Type> FieldTypes => _fieldTypes;

    public Type RepoEntityType { get; private set; }
    private Dictionary<string, Type> _fieldTypes;

    private Dictionary<string, IEntityVarMeta<T>> _vars;

    public void ForReference()
    {
        return;
        new EntityMeta<T>();
    }
    public EntityMeta()
    {
        var entityType = typeof(T);
        //bc with generic parameters it will not capture all the classes
        if (entityType.ContainsGenericParameters) 
            throw new Exception();
        RepoEntityType = (Type) entityType
            .GetMethod(nameof(Entity.GetRepoEntityType))
            .Invoke(null, null);
        
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        _fieldNames = properties.Select(p => p.Name).ToList();
        _fieldTypes = properties.ToDictionary(p => p.Name, p => p.PropertyType);
        _vars = new Dictionary<string, IEntityVarMeta<T>>();
        
        var makeFuncsMi = typeof(EntityMeta<T>).GetMethod(nameof(MakeFuncs),
            BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var propertyInfo in properties)
        {
            var makeFuncsGeneric = makeFuncsMi.MakeGenericMethod(propertyInfo.PropertyType);
            makeFuncsGeneric.Invoke(this, new []{propertyInfo});
        }
    }

    private void MakeFuncs<TProperty>(PropertyInfo prop)
    {
        var name = prop.Name;
        var type = prop.PropertyType;

        var mi = GetType().GetMethod(nameof(SetupVarType), BindingFlags.Instance | BindingFlags.NonPublic);
        var genericMi = mi.MakeGenericMethod(new[] {typeof(TProperty)});
        genericMi.Invoke(this, new []{prop});
    }
    private void SetupVarType<TProperty>(PropertyInfo prop)
    {
        var eVar = new EntityVarMeta<T, TProperty>(prop);
        _vars.Add(prop.Name, eVar);
    }
    
    public object[] GetPropertyValues(Entity entity)
    {
        var t = (T) entity;
        var args = new object[_fieldNames.Count];
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            var fieldName = _fieldNames[i];
            args[i] = _vars[fieldName].GetForSerialize(t);
        }

        return args;
    }

    public IRefCollection GetRefCollection(string fieldName, Entity t, ProcedureWriteKey key)
    {
        return (IRefCollection)_vars[fieldName].GetForSerialize((T)t);
    }

    public void AddToData(Entity e, StrongWriteKey key)
    {
        key.Data.AddEntity<T>((T)e, key, RepoEntityType);
    }
    public void RemoveFromData(Entity e, StrongWriteKey key)
    {
        key.Data.RemoveEntity<T>((T)e, key);
    }
    public void UpdateEntityVarServer<TProperty>(string fieldName, Entity t, ServerWriteKey key, TProperty newValue)
    {
        var repo = key.Data.EntityRepoIndex[t.Id];
        
        var prop = _vars[fieldName].GetForSerialize((T)t);
        if (prop is TProperty == false)
        {
            GD.Print($"{fieldName} is not {typeof(TProperty)}");
        }
        var oldValue = (TProperty)prop;
        _vars[fieldName].Set((T)t, newValue, key);
        ValueChangedHandler<T, TProperty>.Raise(fieldName, (T)t, oldValue, newValue, key);
    }
    public void UpdateEntityVar<TProperty>(string fieldName, Entity t, StrongWriteKey key, TProperty newValue)
    {
        var oldValue = (TProperty)_vars[fieldName].GetForSerialize((T)t);
        _vars[fieldName].Set((T)t, newValue, key);
        ValueChangedHandler<T, TProperty>.Raise(fieldName, (T)t, oldValue, newValue, key);
        if (key is HostWriteKey hKey)
        {
            var bytes = Game.I.Serializer.MP.Serialize(newValue);
            hKey.HostServer.QueueUpdate(EntityVarUpdate.Create(fieldName, t.Id, bytes, hKey));
        }
    }
}
