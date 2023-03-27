using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityMeta<T> : IEntityMeta where T : Entity
{
    public Type EntityType => typeof(T);
    public Type DomainType { get; private set; }
    public IReadOnlyList<string> FieldNameList => _fieldNames;
    public HashSet<string> FieldNameHash { get; }
    private List<string> _fieldNames;
    public IReadOnlyDictionary<string, Type> FieldTypes => _fieldTypes;

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
        {
            throw new SerializationException($"Entity {entityType.Name} cannot have generic parameters");
        }
        
        DomainType = (Type)entityType
            .GetMethod(nameof(DomainType), BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, null);
        
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        _fieldNames = properties.Select(p => p.Name).ToList();
        FieldNameHash = _fieldNames.ToHashSet();
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

    public void UpdateEntityVarServer<TProperty>(string fieldName, Entity t, ServerWriteKey key, TProperty newValue)
    {
        var prop = _vars[fieldName].GetForSerialize((T)t);
        if (prop is TProperty == false)
        {
            throw new SerializationException($"{fieldName} is not {typeof(TProperty)}");
        }
        var oldValue = (TProperty)prop;
        _vars[fieldName].Set((T)t, newValue, key);
        key.Data.EntityTypeTree[typeof(T)]
            .Propagate(new ValChangeNotice<TProperty>(t, fieldName, newValue, oldValue));
        
    }
    public void UpdateEntityVar<TProperty>(string fieldName, Entity t, StrongWriteKey key, TProperty newValue)
    {
        var oldValue = (TProperty)_vars[fieldName].GetForSerialize((T)t);
        _vars[fieldName].Set((T)t, newValue, key);
        var notice = new ValChangeNotice<TProperty>(t, fieldName, newValue, oldValue);
        key.Data.EntityTypeTree[t.GetType()].EntityValChanged.HandleChange(notice);
        if (key is HostWriteKey hKey)
        {
            var bytes = Game.I.Serializer.MP.Serialize(newValue);
            hKey.HostServer.QueueUpdate(EntityVarUpdate.Create(fieldName, t.Id, bytes, hKey));
        }
    }


    public bool TestSerialization(Entity e)
    {
        var t = (T) e;
        return SerializeChecker<T>.Test(t, _vars);
    }
}
