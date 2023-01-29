using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mono.Reflection;

public class EntityMeta<T> : IEntityMeta where T : Entity
{
    public IReadOnlyList<string> FieldNames => _fieldNames;
    private List<string> _fieldNames;
    public Dictionary<string, Type> FieldTypes => _fieldTypes;
    private Dictionary<string, Type> _fieldTypes;

    private Dictionary<string, IEntityVarMeta<T>> _vars;

    private JsonSerializerOptions _options;
    public void ForReference(JsonSerializerOptions options)
    {
        return;
        new EntityMeta<T>(options);
    }
    public EntityMeta(JsonSerializerOptions options)
    {
        _options = options;

        var entityType = typeof(T);
        //bc with generic parameters it will not capture all the classes
        if (entityType.ContainsGenericParameters) 
            throw new Exception(); 
        
        //don't want concrete entity classes having descendents 
        if (entityType.IsSealed == false) 
             throw new Exception();
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

        if (type.HasAttribute<RefAttribute>())
        {
            var underlyingType = type.GetMethod(nameof(IRef<int>.GetUnderlying)).ReturnType;
            var mi = GetType().GetMethod(nameof(SetupRefType), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMi = mi.MakeGenericMethod(new Type[]{typeof(TProperty), underlyingType} );
            genericMi.Invoke(this, new[] {prop} );
        }
        else if (type.HasAttribute<ConvertibleAttribute>())
        {
            var baseType = type.GetMethod(nameof(EntityConvertibleVar<int, int>.ConvertToBase)).ReturnType;
            var convertedType = type.GetMethod(nameof(EntityConvertibleVar<int, int>.ConvertFromBase)).ReturnType;
            var mi = GetType().GetMethod(nameof(SetupConvertibleAttribute),
                BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMi = mi.MakeGenericMethod(new Type[] {typeof(TProperty), baseType, convertedType});
            genericMi.Invoke(this, new[] {prop});
        }
        else
        {
            var mi = GetType().GetMethod(nameof(SetupVarType), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMi = mi.MakeGenericMethod(new[] {typeof(TProperty)});
            genericMi.Invoke(this, new []{prop});
        }
    }
    private void SetupVarType<TProperty>(PropertyInfo prop)
    {
        var eVar = new EntityVarMeta<T, TProperty>(prop);
        _vars.Add(prop.Name, eVar);
    }
    private void SetupRefType<TProperty, TUnderlying>(PropertyInfo prop) where TProperty : IRef<TUnderlying>
    {
        var refVar = new RefVarMeta<T, TProperty, TUnderlying>(prop);
        _vars.Add(prop.Name, refVar);
    }
    private void SetupConvertibleAttribute<TProperty, TBase, TConverted>(PropertyInfo prop)
    {
        var conVar = new ConvertibleVarMeta<T, TProperty, TBase, TConverted>(prop);
        _vars.Add(prop.Name, conVar);
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
    public void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, object newValue)
    {
        _vars[fieldName].Set((T)t, newValue, key);
    }
    public void UpdateEntityVar<TValue>(string fieldName, Entity t, CreateWriteKey key, TValue newValue)
    {
        _vars[fieldName].Set((T)t, newValue, key);
    }
}
