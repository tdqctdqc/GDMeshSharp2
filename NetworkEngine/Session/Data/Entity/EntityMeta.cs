using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mono.Reflection;

public class EntityMeta<T> : IEntityMeta where T : Entity
{
    public IReadOnlyList<string> FieldNames => _fieldNames;
    private List<string> _fieldNames;
    public IReadOnlyDictionary<string, Type> FieldTypes => _fieldTypes;
    private Dictionary<string, Type> _fieldTypes;
    
    //args are val, name, entityId
    private Dictionary<string, Func<string, string, T, object>> _fieldDeserializers; 
    
    private Dictionary<string, Func<object, string>> _fieldSerializers; 
    private Dictionary<string, Func<T, object>> _fieldGetters; 
    private Dictionary<string, Action<T, object>> _fieldInitializers;
    private Dictionary<string, Action<T, ServerWriteKey, string>> _fieldUpdaters;
    private Func<string, T> _deserializer;

    public void ForReference()
    {
        return;
        new EntityMeta<T>();
    }
    public EntityMeta()
    {
        var entityType = typeof(T);
        //bc with generic parameters it will not capture all the classes
        if (entityType.ContainsGenericParameters) throw new Exception(); 
        
        //don't want concrete entity classes having descendents 
        if (entityType.IsSealed == false) 
            throw new Exception();
        
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        _fieldNames = properties.Select(p => p.Name).ToList();
        _fieldNames.Remove("Id");
        _fieldNames.Insert(0, "Id");
        
        _fieldDeserializers = new Dictionary<string, Func<string, string, T, object>>();
        _fieldSerializers = new Dictionary<string, Func<object, string>>();
        _fieldGetters = new Dictionary<string, Func<T, object>>();
        _fieldInitializers = new Dictionary<string, Action<T, object>>();
        _fieldUpdaters = new Dictionary<string, Action<T, ServerWriteKey, string>>();

        foreach (var propertyInfo in properties)
        {
            SetFuncs(entityType, propertyInfo);
        }
        SetConstructor(entityType);
    }

    private void SetFuncs(Type serializableType, PropertyInfo p)
    {
        var name = p.Name;
        var propType = p.PropertyType;
        
        if (propType.HasAttribute<EntityVariableAttribute>() == false) throw new Exception();
        
        _fieldDeserializers[name] = MakeDeserializer(p);
        _fieldSerializers[name] = MakeSerializer(p);
        _fieldGetters[name] = MakeGetter(p);
        _fieldInitializers[name] = MakeSetter(p);
        _fieldUpdaters[name] = MakeUpdater(p);
    }

    private TDelegate MakeCoercedDelegate<TDelegate>(PropertyInfo p, string innerMethodName, MethodInfo propMethod)
        where TDelegate : Delegate
    {
        var innerMethodInfo = typeof(EntityMeta<T>).GetMethod(innerMethodName,
            BindingFlags.Static | BindingFlags.NonPublic);
        var innerGeneric = innerMethodInfo.MakeGenericMethod(p.PropertyType);
        return (TDelegate) innerGeneric.Invoke(null, new object[]{propMethod});
    }
    private static Func<object, string> Test<TProperty>(MethodInfo mi)
    {
        var del = mi.MakeStaticMethodDelegate<Func<TProperty, string>>();
        return (i) => del((TProperty)i);
    }

    private Func<string, string, T, object> MakeDeserializer(PropertyInfo p)
    {
        var deserializeMi = p.PropertyType.GetMethod(EntityVariableAttribute.DeserializeName, BindingFlags.Static | BindingFlags.Public);
        return MakeCoercedDelegate<Func<string, string, T, object>>(p, nameof(MakeDeserializerInner), deserializeMi);
    }
    private static Func<string, string, T, object> MakeDeserializerInner<TProperty>(MethodInfo mi)
    {
        var del = mi.MakeStaticMethodDelegate<Func<string, string, T, TProperty>>();
        return (json, name, entity) => del(json, name, entity);
    }
    private Func<object, string> MakeSerializer(PropertyInfo p)
    {
        var serializeMi = p.PropertyType.GetMethod(EntityVariableAttribute.SerializeName, BindingFlags.Static | BindingFlags.Public);
        return MakeCoercedDelegate<Func<object, string>>(p, nameof(MakeSerializerInner), serializeMi);
    }
    private static Func<object, string> MakeSerializerInner<TProperty>(MethodInfo mi)
    {
        var del = mi.MakeStaticMethodDelegate<Func<TProperty, string>>();
        return (property) => del((TProperty)property);
    }
    private Action<T, object> MakeSetter(PropertyInfo p)
    {
        var mi = p.DeclaringType.GetProperty(p.Name).GetSetMethod(true);
        return MakeCoercedDelegate<Action<T, object>>(p, nameof(MakeSetterInner), mi);
    }
    private static Action<T, object> MakeSetterInner<TProperty>(MethodInfo mi)
    {
        var del = mi.MakeInstanceMethodDelegate<Action<T, TProperty>>();
        return (entity, variable) => del(entity, (TProperty)variable);
    }
    private Func<T, object> MakeGetter(PropertyInfo p)
    {
        var mi = p.GetGetMethod();
        return MakeCoercedDelegate<Func<T, object>>(p, nameof(MakeGetterInner), mi);
    }
    private static Func<T, object> MakeGetterInner<TProperty>(MethodInfo mi)
    {
        
        var getter = mi.MakeInstanceMethodDelegate<Func<T, TProperty>>();
        return entity => getter(entity);
    }

    private Action<T, ServerWriteKey, string> MakeUpdater(PropertyInfo p)
    {
        var innerMethodInfo = typeof(EntityMeta<T>).GetMethod(nameof(MakeUpdaterInner),
            BindingFlags.Instance | BindingFlags.NonPublic);
        var innerGeneric = innerMethodInfo.MakeGenericMethod(p.PropertyType);
        return  (Action<T, ServerWriteKey, string>)innerGeneric.Invoke(this, new object[]{p});
    }

    private Action<T, ServerWriteKey, string> MakeUpdaterInner<TEntityVar>(PropertyInfo p)
    {
        var updater = p.PropertyType.GetMethod("ReceiveUpdate",
            BindingFlags.Static | BindingFlags.Public);
        var propName = p.Name;
        var del = updater.MakeStaticMethodDelegate<Action<TEntityVar, ServerWriteKey, string>>();
        return (entity, key, newVal) =>
        {
            del((TEntityVar)_fieldGetters[propName](entity), key, newVal);
        };
    }
    private void SetConstructor(Type serializableType)
    {
        var constructor = serializableType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(m => m.Name == "DeserializeConstructor")
            .Where(m => m.ReturnType == serializableType)
            .First();
        _deserializer = constructor.MakeStaticMethodDelegate<Func<string, T>>();
    }

    public Entity Deserialize(string json)
    {
        return _deserializer(json);
    }

    public string Serialize(Entity entity)
    {
        var t = (T) entity;
        var jsonArray = new System.Text.Json.Nodes.JsonArray();
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            var fieldName = _fieldNames[i];
            var field = _fieldGetters[fieldName](t);
            jsonArray.Add(_fieldSerializers[fieldName](field));
        }

        return jsonArray.ToJsonString();
    }
    public void Initialize(Entity entity, string json)
    {
        var t = (T) entity;
        var valJsons = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
        
        for (int i = 0; i < valJsons.Count; i++)
        {
            var fieldName = _fieldNames[i];
            var value = _fieldDeserializers[fieldName](valJsons[i], fieldName, t);
            _fieldInitializers[fieldName](t, value);
        }
    }

    public void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, string newValueJson)
    {
        _fieldUpdaters[fieldName]((T) t, key, newValueJson);
    }
}
