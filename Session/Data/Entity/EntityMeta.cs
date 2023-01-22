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
    
    private Dictionary<string, Func<object, string>> _fieldSerializers; 
    private Dictionary<string, Func<T, object>> _fieldGetters; 
    private Dictionary<string, Action<T, string>> _fieldDeserializeAndSetters;
    private Dictionary<string, Action<T, object>> _fieldSetters;
    private Func<string, T> _deserializer;
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
        _fieldNames.Remove("Id");
        _fieldNames.Insert(0, "Id");
        _fieldGetters = new Dictionary<string, Func<T, object>>();
        _fieldDeserializeAndSetters = new Dictionary<string, Action<T, string>>();
        _fieldSerializers = new Dictionary<string, Func<object, string>>();
        _fieldSetters = new Dictionary<string, Action<T, object>>();
        
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
        var getMi = prop.GetGetMethod();
        var getDelType =
            ReflectionExt.MakeCustomDelegateType(typeof(Func<,>), new[] {typeof(T), prop.PropertyType});
        var getterDelg = (Func<T, TProperty>)getMi.MakeInstanceMethodDelegate(getDelType);
        _fieldGetters.Add(name, t => getterDelg(t));
        
        _fieldSerializers.Add(name, p => Serializer.Serialize<TProperty>((TProperty)p));
        
        
        var setter = prop.GetSetMethod(true);

        var setDelgType =   
            ReflectionExt.MakeCustomDelegateType(typeof(Action<,>), new[] {typeof(T), prop.PropertyType});
        var setterDelg = (Action<T, TProperty>) setter.MakeInstanceMethodDelegate(setDelgType);
        _fieldSetters.Add(name, (t, o) => setterDelg(t, (TProperty)o));
        Func<string, TProperty> deserialize = (json) => Serializer.Deserialize<TProperty>(json);
        
        _fieldDeserializeAndSetters.Add(name, (entity, json) =>
        {
            setterDelg(entity, deserialize(json));
        });
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
            var json = _fieldSerializers[fieldName](field);
            jsonArray.Add(json);
        }
        
        return jsonArray.ToJsonString(_options);
    }
    public void Initialize(Entity entity, string json)
    {
        var t = (T) entity;
        var valJsons = Serializer.Deserialize<List<string>>(json);
        
        for (int i = 0; i < valJsons.Count; i++)
        {
            var fieldName = _fieldNames[i];
            _fieldDeserializeAndSetters[fieldName].Invoke(t, valJsons[i]);
        }
    }
    public void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, string newValueJson)
    {
        _fieldDeserializeAndSetters[fieldName]((T) t, newValueJson);
    }

    public void UpdateEntityVar<TValue>(string fieldName, Entity t, CreateWriteKey key, TValue newValue)
    {
        _fieldSetters[fieldName]((T) t, newValue);
    }
    public void SyncEntityRefs(Entity t, ServerWriteKey key)
    {
        foreach (var keyValuePair in _fieldGetters)
        {
            var field = keyValuePair.Value.Invoke((T)t);
            if(field is IEntityRef r) r.SyncRef(key);
            else if (field is IEntityRefCollection c) c.SyncRefs(key);
        }
    }
}
