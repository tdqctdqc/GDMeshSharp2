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
    public IReadOnlyList<Type> FieldTypes => _fieldTypes;
    private List<Type> _fieldTypes;
    
    private Dictionary<string, Func<object, object>> _fieldSerializers; 
    private Dictionary<string, Func<T, object>> _fieldGetters; 
    // private Dictionary<string, Action<T, string>> _fieldDeserializeAndSetters;
    private Dictionary<string, Action<T, object>> _fieldSetters;
    private Dictionary<string, Action<T, object>> _refFieldSetters;
    private Dictionary<string, Action<T, object, StrongWriteKey>> _refUnderlyingSetters;
    private Dictionary<string, Func<T, object>> _refUnderlyingGetters;
    private Dictionary<string, Type> _refUnderlyingTypes;
    private Func<object[], T> _deserializer;
    private JsonSerializerOptions _options;
    public void ForReference(JsonSerializerOptions options)
    {
        return;
        new EntityMeta<T>(options);
    }
    public EntityMeta(JsonSerializerOptions options)
    {
        _options = options;
        GD.Print("handling entity type " + typeof(T).ToString());

        var entityType = typeof(T);
        //bc with generic parameters it will not capture all the classes
        if (entityType.ContainsGenericParameters) 
            throw new Exception(); 
        
        //don't want concrete entity classes having descendents 
        if (entityType.IsSealed == false) 
             throw new Exception();
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        _fieldNames = properties.Select(p => p.Name).ToList();
        _fieldTypes = properties.Select(p => p.PropertyType).ToList();
        _fieldGetters = new Dictionary<string, Func<T, object>>();
        // _fieldDeserializeAndSetters = new Dictionary<string, Action<T, string>>();
        _fieldSerializers = new Dictionary<string, Func<object, object>>();
        _fieldSetters = new Dictionary<string, Action<T, object>>();
        _refUnderlyingTypes = new Dictionary<string, Type>();
        _refUnderlyingSetters = new Dictionary<string, Action<T, object, StrongWriteKey>>();
        _refFieldSetters = new Dictionary<string, Action<T, object>>();
        _refUnderlyingGetters = new Dictionary<string, Func<T, object>>();
        
        var makeFuncsMi = typeof(EntityMeta<T>).GetMethod(nameof(MakeFuncs),
            BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var propertyInfo in properties)
        {
            var makeFuncsGeneric = makeFuncsMi.MakeGenericMethod(propertyInfo.PropertyType);
            makeFuncsGeneric.Invoke(this, new []{propertyInfo});
        }
        SetConstructor(entityType);
    }

    private void MakeFuncs<TProperty>(PropertyInfo prop)
    {
        var name = prop.Name;
        var type = prop.PropertyType;
        var getMi = prop.GetGetMethod();
        var getterDelg = getMi.MakeInstanceMethodDelegate<Func<T, TProperty>>();
        _fieldGetters.Add(name, t => getterDelg(t));

        if (type.HasAttribute<RefAttribute>())
        {
            var underlyingType = type.GetMethod(nameof(IRef<int>.GetUnderlying)).ReturnType;
            var mi = GetType().GetMethod(nameof(SetupRefType), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMi = mi.MakeGenericMethod(new Type[]{typeof(TProperty), underlyingType} );
            genericMi.Invoke(this, new []{prop} );
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
        var name = prop.Name;
        _fieldSerializers.Add(name, p => (TProperty)p);
        var setter = prop.GetSetMethod(true);
        var setterDelg = setter.MakeInstanceMethodDelegate<Action<T, TProperty>>();
        _fieldSetters.Add(name, (t, o) => setterDelg(t, (TProperty)o));
    }
    private void SetupRefType<TProperty, TUnderlying>(PropertyInfo prop) where TProperty : IRef<TUnderlying>
    {
        _refUnderlyingTypes[prop.Name] = typeof(TUnderlying);
        var name = prop.Name;
        var setUnderlyingMi = typeof(TProperty).GetMethod(nameof(IRef<int>.Set));
        var setUnderlyingDel = setUnderlyingMi.MakeInstanceMethodDelegate<Action<TProperty, TUnderlying, StrongWriteKey>>();
        _refUnderlyingSetters[name] = (t, o, k) => 
            setUnderlyingDel((TProperty)_fieldGetters[name](t), (TUnderlying)o, k);


        
        var setter = prop.GetSetMethod(true);
        var setterDelg = setter.MakeInstanceMethodDelegate<Action<T, TProperty>>();

        var deserializeConstructorMi = prop.PropertyType.GetMethod(nameof(EntityRef<Player>.DeserializeConstructor),
            BindingFlags.Static | BindingFlags.Public);
        var deserializeConstructorDel = deserializeConstructorMi.MakeInstanceMethodDelegate<Func<TUnderlying, TProperty>>();
        
        _refFieldSetters.Add(name, (t, o) => setterDelg(t, deserializeConstructorDel((TUnderlying)o)));

        var getUnderlyingMi = typeof(TProperty).GetMethod(nameof(IRef<int>.GetUnderlying));
        var getUnderlyingDel = getUnderlyingMi.MakeInstanceMethodDelegate<Func<TProperty, TUnderlying>>();
        
        _refUnderlyingGetters.Add(name, t => getUnderlyingDel((TProperty)_fieldGetters[name](t)));
        _fieldSerializers.Add(name, p => getUnderlyingDel((TProperty) p));
    }
    private void SetConstructor(Type serializableType)
    {
        var constructor = serializableType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(m => m.Name == "DeserializeConstructor")
            .Where(m => m.ReturnType == serializableType)
            .First();
        _deserializer = constructor.MakeStaticMethodDelegate<Func<object[], T>>();
    }
    public Entity Deserialize(object[] args)
    {
        return _deserializer(args);
    }

    public object[] GetArgs(Entity entity)
    {
        var t = (T) entity;
        var args = new object[_fieldNames.Count];
        var jsonArray = new System.Text.Json.Nodes.JsonArray();
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            var fieldName = _fieldNames[i];
            var fieldType = _fieldTypes[i];
            if (fieldType.HasAttribute<RefAttribute>())
            {
                var arg = _refUnderlyingGetters[fieldName]((T) entity);
                args[i] = arg;
            }
            else
            {
                var arg = _fieldGetters[fieldName]((T) entity);
                args[i] = arg;
            }
        }

        return args;
    }
    public void Initialize(Entity entity, object[] args)
    {
        var t = (T) entity;
        
        for (int i = 0; i < args.Length; i++)
        {
            var fieldName = _fieldNames[i];
            _fieldSetters[fieldName].Invoke(t, args[i]);
        }
    }
    public void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, object newValue)
    {
        _fieldSetters[fieldName]((T) t, newValue);
    }
    public void UpdateEntityVar<TValue>(string fieldName, Entity t, CreateWriteKey key, TValue newValue)
    {
        _fieldSetters[fieldName]((T) t, newValue);
    }
    public void UpdateEntityRefVar(string fieldName, Entity t, ServerWriteKey key, object newValue)
    {
        //generally use procedures instead
        _refUnderlyingSetters[fieldName].Invoke((T)t, newValue, key);
    }
    public void UpdateEntityRefVar<TUnderlying>(string fieldName, Entity t, CreateWriteKey key, TUnderlying newValue)
    {
        var refer = (IRef<TUnderlying>)_fieldGetters[fieldName]((T) t);
        refer.Set(newValue, key);
    }
}
