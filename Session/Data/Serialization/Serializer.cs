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

    private Dictionary<string, Type> _entityTypes;
    private Dictionary<Type, IEntityMeta> _entityMetas;
    public IEntityMeta GetEntityMeta(Type type) => _entityMetas[type];
    public IEntityMeta GetEntityMeta(string type) => _entityMetas[_entityTypes[type]];
    
    public EntityMeta<T> GetEntityMeta<T>() where T : Entity
    {
        return (EntityMeta<T>)_entityMetas[typeof(T)];
    }

    
    public Dictionary<string, Type> DomainTypes { get; private set; }



    private Dictionary<string, Type> _updateTypes;
    private Dictionary<Type, IUpdateMeta> _updateMetas;
    public IUpdateMeta GetUpdateMeta(Type type) => _updateMetas[type];
    public IUpdateMeta GetUpdateMeta(string type) => _updateMetas[_updateTypes[type]];
    public UpdateMeta<TUpdate> GetUpdateMeta<TUpdate>() where TUpdate : Update
    {
        return (UpdateMeta<TUpdate>)_updateMetas[typeof(TUpdate)];
    }
    
    
    
    private Dictionary<string, Type> _procedureTypes;
    private Dictionary<Type, IProcedureMeta> _procedureMetas;
    public IProcedureMeta GetProcedureMeta(Type type) => _procedureMetas[type];
    public IProcedureMeta GetProcedureMeta(string type) => _procedureMetas[_procedureTypes[type]];
    public ProcedureMeta<TProcedure> GetProcedureMeta<TProcedure>() where TProcedure : Procedure
    {
        return (ProcedureMeta<TProcedure>)_procedureMetas[typeof(TProcedure)];
    }


    public Serializer()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new Vector2JsonConverter());
        
        SetupEntityMetas();
        SetupUpdateMetas();
        SetupProcedureMetas();
            
        DomainTypes = new Dictionary<string, Type>();
        var domainTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Domain>();
        foreach (var domainType in domainTypes)
        {
            DomainTypes.Add(domainType.Name, domainType);
        }
        
        CommandMeta.Setup();
    }

    private void SetupEntityMetas()
    {
        _entityTypes = new Dictionary<string, Type>();
        var reference = nameof(EntityMeta<Entity>.ForReference);
        _entityMetas = new Dictionary<Type, IEntityMeta>();
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

            _entityTypes.Add(entityType.Name, entityType);
            var genericMeta = metaTypes.MakeGenericType(entityType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            _entityMetas.Add(entityType, (IEntityMeta)meta);
        }
    }

    private void SetupUpdateMetas()
    {
        _updateTypes = new Dictionary<string, Type>();
        var reference = nameof(UpdateMeta<EntityVarUpdate>.ForReference);
        _updateMetas = new Dictionary<Type, IUpdateMeta>();
        var updateTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Update>();
        var metaTypes = typeof(UpdateMeta<>);
        foreach (var updateType in updateTypes)
        {
            _updateTypes.Add(updateType.Name, updateType);
            var genericMeta = metaTypes.MakeGenericType(updateType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            _updateMetas.Add(updateType, (IUpdateMeta)meta);
        }
    }
    private void SetupProcedureMetas()
    {
        _procedureTypes = new Dictionary<string, Type>();
        var reference = nameof(ProcedureMeta<Procedure>.ForReference);
        _procedureMetas = new Dictionary<Type, IProcedureMeta>();
        var procedureTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Procedure>();
        var metaTypes = typeof(ProcedureMeta<>);
        foreach (var procType in procedureTypes)
        {
            _procedureTypes.Add(procType.Name, procType);
            var genericMeta = metaTypes.MakeGenericType(procType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{_options});
            _procedureMetas.Add(procType, (IProcedureMeta)meta);
        }
    }
    public string Serialize<TValue>(TValue t)
    {
        return JsonSerializer.Serialize<TValue>(t, _options);
    }
    public TValue Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, _options);
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
        writer.WriteNumberValue(value.Count);
        foreach (var i in value)
        {
            writer.WriteNumberValue(i);
        }
        writer.WriteEndArray();
    }
}
