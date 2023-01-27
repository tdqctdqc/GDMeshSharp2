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

    
    public Dictionary<string, Type> DomainTypes { get; private set; }

    private Dictionary<Type, IMeta<Update>> _updateMetas;
    public IMeta<Update> GetUpdateMeta(Type type) => _updateMetas[type];
    public IMeta<Update> GetUpdateMeta(string type) => _updateMetas[Types[type]];
    public Meta<TUpdate, Update> GetUpdateMeta<TUpdate>() where TUpdate : Update
    {
        return (Meta<TUpdate, Update>)_updateMetas[typeof(TUpdate)];
    }
    
    private Dictionary<Type, IMeta<Procedure>> _procedureMetas;
    public IMeta<Procedure> GetProcedureMeta(Type type) => _procedureMetas[type];
    public IMeta<Procedure> GetProcedureMeta(string type) => _procedureMetas[Types[type]];
    public Meta<TProcedure, Procedure> GetProcedureMeta<TProcedure>() where TProcedure : Procedure
    {
        return (Meta<TProcedure, Procedure>)_procedureMetas[typeof(TProcedure)];
    }
    
    
    private Dictionary<Type, IMeta<Command>> _commandMetas;
    public IMeta<Command> GetCommandMeta(Type type) => _commandMetas[type];
    public IMeta<Command> GetCommandMeta(string type) => _commandMetas[Types[type]];
    public Meta<TCommand, Command> GetCommandMeta<TCommand>() where TCommand : Command
    {
        return (Meta<TCommand, Command>)_commandMetas[typeof(TCommand)];
    }

    public Serializer()
    {
        _options = new JsonSerializerOptions();
        // _options.Converters.Add(new Vector2JsonConverter());
        
        SetupEntityMetas();
        _updateMetas = new Dictionary<Type, IMeta<Update>>();
        SetupMetas<Update>(_updateMetas);
        _procedureMetas = new Dictionary<Type, IMeta<Procedure>>();
        SetupMetas<Procedure>(_procedureMetas);

        _commandMetas = new Dictionary<Type, IMeta<Command>>();
        SetupMetas<Command>(_commandMetas);
            
        DomainTypes = new Dictionary<string, Type>();
        var domainTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Domain>();
        foreach (var domainType in domainTypes)
        {
            DomainTypes.Add(domainType.Name, domainType);
        }
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
            // var refConverterType = typeof(EntityRefJsonConverter<>).MakeGenericType(entityType);
            // var refColConverterType = typeof(EntityRefColJsonConverter<>).MakeGenericType(entityType);
            // var refConverter = (JsonConverter)refConverterType.GetConstructors()[0].Invoke(null);
            // var refColConverter = (JsonConverter)refColConverterType.GetConstructors()[0].Invoke(null);
            // _options.Converters.Add(refConverter);
            // _options.Converters.Add(refColConverter);

            Types.Add(entityType.Name, entityType);
            var genericMeta = metaTypes.MakeGenericType(entityType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            _entityMetas.Add(entityType, (IEntityMeta)meta);
        }
    }
    private void SetupMetas<TMeta>(Dictionary<Type, IMeta<TMeta>> metaDic)
    {
        var reference = nameof(Meta<TMeta, TMeta>.ForReference);
        var updateTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<TMeta>();
        var metaTypes = typeof(Meta<,>);
        foreach (var updateType in updateTypes)
        {
            Types.Add(updateType.Name, updateType);
            var genericMeta = metaTypes.MakeGenericType(updateType, typeof(TMeta));
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            metaDic.Add(updateType, (IMeta<TMeta>)meta);
        }
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
        writer.WriteNumberValue(value.Count());
        foreach (var i in value.RefIds)
        {
            writer.WriteNumberValue(i);
        }
        writer.WriteEndArray();
    }
}
