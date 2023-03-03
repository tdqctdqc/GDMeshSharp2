using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

public class Serializer
{
    public MessagePackManager MP { get; private set; }
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
        MP = new MessagePackManager();
        MP.Setup();
        
        SetupEntityMetas();
        AddTypeToDic<Update>();
        AddTypeToDic<Procedure>();
        AddTypeToDic<Command>();
        AddTypeToDic<Domain>();
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
            var meta = constructor.Invoke(new object[]{});
            _entityMetas.Add(entityType, (IEntityMeta)meta);
        }
    }
    private void AddTypeToDic<T>()
    {
        var metaSubTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<T>();
        foreach (var metaSubType in metaSubTypes)
        {
            Types.Add(metaSubType.Name, metaSubType);
        }
    }

    public void TestSerialization(HostWriteKey key)
    {
        foreach (var keyValuePair in key.Data.Domains)
        {
            foreach (var valueRepo in keyValuePair.Value.Repos)
            {
                var e = valueRepo.Value.Entities.FirstOrDefault();
                
                if(e != null)
                {
                    TestEntitySerialization(e, key);
                }
            }
        }
    }
    
    private void TestEntitySerialization(Entity e, HostWriteKey key)
    {
        GD.Print("testing serialization for " + e.GetType().Name);
        var eType = e.GetType();
        var props = e.GetType().GetProperties();
        foreach (var p in props)
        {
            TestProperty(p, e);
        }
        
        var eBytes = Game.I.Serializer.MP.Serialize(e, eType);
        var e2 = (Entity)Game.I.Serializer.MP.Deserialize(eBytes, eType);
        
        var u = EntityCreationUpdate.Create(e, key);
        var uBytes = Game.I.Serializer.MP.Serialize<EntityCreationUpdate>(u);
        var u2 = Game.I.Serializer.MP.Deserialize<EntityCreationUpdate>(uBytes);
        for (var j = 0; j < u.EntityBytes.Length; j++)
        {
            if (u.EntityBytes[j] != u2.EntityBytes[j])
            {
                throw new Exception();
            }
        }
        var e3 = (Entity)Game.I.Serializer.MP.Deserialize(u2.EntityBytes, eType);
        foreach (var p in props)
        {
            TestPropertySerialization<Entity>(p, e, e2, e3);
        }
    }
    private void TestProperty<TProperty>(PropertyInfo prop, Entity e)
    {

        GD.Print("\ttesting arg " + typeof(TProperty) + " " + prop.Name);
            
        var get = prop.GetGetMethod();
        var arg = get.Invoke(e, null);
        if (arg.GetType() != typeof(TProperty)) throw new Exception();
        var argBytes = MP.Serialize<TProperty>((TProperty)arg);
        var arg2 = MP.Deserialize<TProperty>(argBytes);
        
        if (typeof(TProperty).IsClass)
        {
            if ((arg == null) != (arg2 == null)) throw new Exception();
        }
        else
        {
        }
    }
    private void TestProperty(PropertyInfo prop, Entity e)
    {
        var pType = prop.PropertyType;

        GD.Print("\ttesting arg " + pType + " " + prop.Name);
        var get = prop.GetGetMethod();
        var arg = get.Invoke(e, null);
        if (arg == null) return;
        if (arg.GetType() != pType)
        {
            GD.Print(arg.GetType());
            GD.Print(pType);
            throw new Exception();
        }
        var argBytes = MP.Serialize(arg, pType);
        var arg2 = MP.Deserialize(argBytes, pType);
        
        if (pType.IsClass)
        {
            if ((arg == null) != (arg2 == null)) throw new Exception();
        }
        else
        {
        }
    }
    private void TestPropertySerialization<TProperty, TEntity>(PropertyInfo prop, 
        TEntity e1, TEntity e2, TEntity e3)
    {
        GD.Print("\ttesting arg " + prop.Name);
            
        var get = prop.GetGetMethod();
        var pType = prop.PropertyType;
        var arg = (TProperty)get.Invoke(e1, null);
        var arg2 = (TProperty)get.Invoke(e2, null);
        var arg3 = (TProperty)get.Invoke(e3, null);
        var argBytes = MP.Serialize<TProperty>(arg);
        var arg4 = MP.Deserialize<TProperty>(argBytes);
        
        if (pType.IsClass)
        {
            if (arg == null != (arg3 == null)) throw new Exception();
            if (arg == null != (arg2 == null)) throw new Exception();
            if (arg == null != (arg4 == null)) throw new Exception();
        }
        else
        {
            if (pType == typeof(Color)) return;
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
    private void TestPropertySerialization<THolder>(PropertyInfo prop, 
        THolder e1, THolder e2, THolder e3)
    {
        GD.Print("\ttesting arg " + prop.Name);
            
        var get = prop.GetGetMethod();
        var pType = prop.PropertyType;
        
        var arg = get.Invoke(e1, null);
        if (arg == null) return;
        
        var arg2 = get.Invoke(e2, null);
        if (arg2 == null) return;

        var arg3 = get.Invoke(e3, null);
        if (arg3 == null) return;

        var argBytes = MP.Serialize(arg, pType);
        var arg4 = MP.Deserialize(argBytes, pType);
        
        if (pType.IsClass)
        {
            if (arg == null != (arg3 == null)) throw new Exception();
            if (arg == null != (arg2 == null)) throw new Exception();
            if (arg == null != (arg4 == null)) throw new Exception();
            
            TestStructPropEquality(arg, arg2);
            TestStructPropEquality(arg, arg3);
            TestStructPropEquality(arg, arg4);
        }
        else
        {
            if (arg.Equals(arg2) == false)
            {
                throw new Exception();
            }
            if (arg.Equals(arg3) == false)
            {
                throw new Exception();
            }
            if (arg.Equals(arg4) == false) 
            {
                throw new Exception();
            }
        }
    }

    private void TestStructPropEquality(object arg1, object arg2)
    {
        var p1s = arg1.GetType().GetProperties().Where(p => p.PropertyType.IsValueType).ToList();
        var p2s = arg2.GetType().GetProperties().Where(p => p.PropertyType.IsValueType).ToList();
        if (p1s.Count != p2s.Count) throw new Exception();
        for (int i = 0; i < p1s.Count; i++)
        {
            if (p1s[i].Equals(p2s[i]) == false) throw new Exception();
        }
    }
    private bool IsGoodConstructor(ConstructorInfo c, Type t)
    {
        // if (c.HasAttribute<JsonConstructorAttribute>() == false) return false;
        // var propTypes = t.GetProperties().ToDictionary(p => p.Name.ToLower(), p => p.PropertyType);
        // var cArgs = c.GetParameters();
        // foreach (var parameterInfo in cArgs)
        // {
        //     var lower = parameterInfo.Name.ToLower();
        //     if (propTypes.ContainsKey(lower) == false) return false;
        //     if (propTypes[lower] != parameterInfo.ParameterType) return false;
        // }

        return true;
    }

        
}



