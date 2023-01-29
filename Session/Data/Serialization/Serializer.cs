using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

public class Serializer
{
    private JsonSerializerOptions _options;
    public Dictionary<string, Type> Types { get; private set; }
    public Dictionary<Type, IEntityMeta> _entityMetas;
    public IEntityMeta GetEntityMeta(Type type) => _entityMetas[type];
    public IEntityMeta GetEntityMeta(string typeName)  
    {
        if (Types.ContainsKey(typeName) == false)
        {
            throw new Exception("no entity types of name " + typeName);
        }
        if (_entityMetas.ContainsKey(Types[typeName]) == false)
        {
            throw new Exception("no entity types of type " + Types[typeName].Name);
        }
        if (_entityMetas[Types[typeName]] == null)
        {
            throw new Exception("null meta for " + Types[typeName].Name);
        }
        return _entityMetas[Types[typeName]];
    }
    public EntityMeta<T> GetEntityMeta<T>() where T : Entity
    {
        return (EntityMeta<T>)_entityMetas[typeof(T)];
    }

    public Serializer()
    {
        _options = new JsonSerializerOptions
        {
        };
        _options.Converters.Add(new Vector2JsonConverter());
        _options.Converters.Add(new ColorJsonConverter());
        
        SetupEntityMetas();
        SetupTypes<Update>();
        SetupTypes<Procedure>();
        SetupTypes<Command>();
        SetupTypes<Domain>();
    }

    private void SetupEntityMetas()
    {
        Types = new Dictionary<string, Type>();
        var reference = nameof(EntityMeta<Entity>.ForReference);
        _entityMetas = new Dictionary<Type, IEntityMeta>();
        var entityTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Entity>();
        var metaTypes = typeof(EntityMeta<>);
        foreach (var entityType in entityTypes)
        {

            Types.Add(entityType.Name, entityType);
            var genericMeta = metaTypes.MakeGenericType(entityType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            _entityMetas.Add(entityType, (IEntityMeta)meta);
        }
    }
    private void SetupTypes<TMeta>()
    {
        var metaSubTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<TMeta>();
        foreach (var metaSubType in metaSubTypes)
        {
            Types.Add(metaSubType.Name, metaSubType);
        }
    }

    public void TestSerialization(Data data, HostWriteKey key)
    {
        foreach (var keyValuePair in data.Domains)
        {
            foreach (var valueRepo in keyValuePair.Value.Repos)
            {
                var e = valueRepo.Value.Entities.FirstOrDefault();
                if(e != null) TestEntitySerialization(e, key);
            }
        }
    }
    private void TestEntitySerialization(Entity e, HostWriteKey key)
    {
        GD.Print("testing serialization for " + e.GetType().Name);

        var constructors = e.GetType().GetConstructors();
        if (constructors.Any(c => IsGoodConstructor(c, e.GetType())) == false) throw new Exception();
        
        var eBytes = Game.I.Serializer.SerializeToUtf8(e);
        var e2 = Game.I.Serializer.Deserialize(eBytes, e.GetType());
        var props = e.GetType().GetProperties();
        
        var u = new EntityCreationUpdate(e.GetType(), typeof(PlanetDomain), e, key);
        var uBytes = Game.I.Serializer.SerializeToUtf8(u);
        var u2 = JsonSerializer.Deserialize<EntityCreationUpdate>(uBytes);
        for (var j = 0; j < u.EntityBytes.Length; j++)
        {
            if (u.EntityBytes[j] != u2.EntityBytes[j])
            {
                throw new Exception();
            }
        }
        var e3 = Game.I.Serializer.Deserialize(u2.EntityBytes, e.GetType());
        
        
        foreach (var p in props)
        {
            GD.Print("\ttesting arg " + p.Name);
            
            var get = p.GetGetMethod();
            var pType = p.PropertyType;
            var arg = get.Invoke(e, null);
            var arg2 = get.Invoke(e2, null);
            var arg3 = get.Invoke(e3, null);
            var argBytes = Game.I.Serializer.SerializeToUtf8(arg);
            var arg4 = Game.I.Serializer.Deserialize(argBytes, pType);

            if (pType.IsClass)
            {
                if (arg is null != arg2 is null) throw new Exception();
                if (arg is null != arg3 is null) throw new Exception();
                if (arg is null != arg4 is null) throw new Exception();
            }
            else
            {
                if (pType == typeof(Color)) continue;
                if (arg.Equals(arg2) == false)
                {
                    throw new Exception();
                }
                if (arg.Equals(arg3) == false)
                {
                    throw new Exception();
                }
                if (arg.Equals(arg3) == false) 
                {
                    throw new Exception();
                }
            }
        }
    }

    private bool IsGoodConstructor(ConstructorInfo c, Type t)
    {
        if (c.HasAttribute<JsonConstructorAttribute>() == false) return false;
        var propTypes = t.GetProperties().ToDictionary(p => p.Name.ToLower(), p => p.PropertyType);
        var cArgs = c.GetParameters();
        foreach (var parameterInfo in cArgs)
        {
            var lower = parameterInfo.Name.ToLower();
            if (propTypes.ContainsKey(lower) == false) return false;
            if (propTypes[lower] != parameterInfo.ParameterType) return false;
        }

        return true;
    }
    public string Serialize<TValue>(TValue t)
    {
        return JsonSerializer.Serialize<TValue>(t, _options);
    }

    public byte[] SerializeToUtf8(object ob)
    {
        return JsonSerializer.SerializeToUtf8Bytes(ob, _options);
    }
    public TValue Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, _options);
    }
    public TValue Deserialize<TValue>(byte[] json)
    {
        return JsonSerializer.Deserialize<TValue>(json, _options);
    }
    public object Deserialize(byte[] json, Type objectType)
    {
        return JsonSerializer.Deserialize(json, objectType, _options);
    }
    public object Deserialize(string json, Type objectType)
    {
        return JsonSerializer.Deserialize(json, objectType, _options);
    }
}



public class PrivateConstructorContractResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object && jsonTypeInfo.CreateObject is null)
        {
            if (typeof(Entity).IsAssignableFrom(jsonTypeInfo.Type))
            {
                jsonTypeInfo.CreateObject = () =>
                    System.Runtime.Serialization.FormatterServices.GetUninitializedObject(jsonTypeInfo.Type);
            }
        }

        return jsonTypeInfo;
    }
}
