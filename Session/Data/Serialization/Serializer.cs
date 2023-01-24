using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public class Serializer
{
    public static Dictionary<string, Type> EntityTypes { get; private set; }
    public static Dictionary<string, Type> DomainTypes { get; private set; }
    private static Dictionary<Type, Func<string, object>> _deserializers;
    private static Dictionary<Type, Func<object, string>> _serializers;
    private static Dictionary<Type, IEntityMeta> _serializableMetas;
    public static IEntityMeta GetEntityMeta(Type type) => _serializableMetas[type];
    private static JsonSerializerOptions _options;
    public static EntityMeta<T> GetEntityMeta<T>() where T : Entity
    {
        return (EntityMeta<T>)_serializableMetas[typeof(T)];
    }
    public static void Setup()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new Vector2JsonConverter());
        
        EntityTypes = new Dictionary<string, Type>();
        var reference = nameof(EntityMeta<Entity>.ForReference);
        _serializableMetas = new Dictionary<Type, IEntityMeta>();
        var entityTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Entity>();
        var metaTypes = typeof(EntityMeta<>);
        foreach (var entityType in entityTypes)
        {
            var refConverterType = typeof(EntityRefJsonConverter<>).MakeGenericType(entityType);
            var refColConverterType = typeof(EntityRefColJsonConverter<>).MakeGenericType(entityType);
            var refConverter = (JsonConverter)refConverterType.GetConstructors()[0].Invoke(null);
            var refColConverter = (JsonConverter)refColConverterType.GetConstructors()[0].Invoke(null);
            _options.Converters.Add(refConverter);
            _options.Converters.Add(refColConverter);

            EntityTypes.Add(entityType.Name, entityType);
            var genericMeta = metaTypes.MakeGenericType(entityType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            _serializableMetas.Add(entityType, (IEntityMeta)meta);
        }

        DomainTypes = new Dictionary<string, Type>();
        var domainTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Domain>();
        foreach (var domainType in domainTypes)
        {
            DomainTypes.Add(domainType.Name, domainType);
        }
        
        ProcedureMeta.Setup();
        CommandMeta.Setup();
    }

    public static string Serialize<TValue>(TValue t)
    {
        return JsonSerializer.Serialize<TValue>(t, _options);
    }
    public static TValue Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, _options);
    }
}

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();
        float x = reader.GetSingle();

        reader.Read();
        reader.Read();
        float y = reader.GetSingle();
        reader.Read();
        
        return new Vector2(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.x);
        writer.WriteNumber("y", value.y);
        writer.WriteEndObject();
    }
}

public class EntityRefJsonConverter<TEntity> : JsonConverter<EntityRef<TEntity>> where TEntity : Entity
{
    public override EntityRef<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        reader.Read();
        int id = reader.GetInt32();
        reader.Read();
        return new EntityRef<TEntity>(id);
    }

    public override void Write(Utf8JsonWriter writer, EntityRef<TEntity> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.RefId);
        writer.WriteEndArray();
    }
}

public class EntityRefColJsonConverter<TEntity> : JsonConverter<EntityRefCollection<TEntity>> where TEntity : Entity
{
    public override EntityRefCollection<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        reader.Read();
        var refIds = new List<int>();
        int count = reader.GetInt32();
        reader.Read();

        for (int i = 0; i < count; i++)
        {
            var read = reader.GetInt32();
            reader.Read();
            refIds.Add(read);
        }
        reader.Read();
        GD.Print(refIds.ToArray());
        return new EntityRefCollection<TEntity>(refIds);
    }

    public override void Write(Utf8JsonWriter writer, EntityRefCollection<TEntity> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Count);
        foreach (var i in value)
        {
            writer.WriteNumberValue(i);
        }
        writer.WriteEndArray();
    }
}
