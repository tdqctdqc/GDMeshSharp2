using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;

public class Meta<TImplem, TMeta> : IMeta<TMeta> where TImplem : TMeta
{
    private JsonSerializerOptions _options;
    private List<string> _fieldNames;
    private Dictionary<string, Type> _fieldTypes;
    private Dictionary<string, Func<TImplem, object>> _fieldGetters;
    private Dictionary<string, Action<TImplem, object>> _fieldSetters;
    private Func<object[], TImplem> _deserializeConstructor;
    public Meta(JsonSerializerOptions options)
    {
        return;
        _options = options;
        _fieldGetters = new Dictionary<string, Func<TImplem, object>>();
        _fieldSetters = new Dictionary<string, Action<TImplem, object>>();
        var type = typeof(TImplem);
        var deserializeMi =
            type.GetMethod("DeserializeConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        var deserializeDel = deserializeMi.MakeInstanceMethodDelegate<Func<object[], TImplem>>();
        _deserializeConstructor = deserializeDel;

        var properties = type.GetProperties();
        _fieldNames = properties.Select(p => p.Name).ToList();
        _fieldTypes = properties.ToDictionary(p => p.Name, p => p.PropertyType);
        var setFuncsMi = GetType().GetMethod(nameof(SetFuncs), BindingFlags.Instance | BindingFlags.NonPublic);
        for (var i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            var setFuncsGeneric = setFuncsMi.MakeGenericMethod(new[] {prop.PropertyType});
            setFuncsGeneric.Invoke(this, new[] {prop});
        }
    }

    private void SetFuncs<TProperty>(PropertyInfo p)
    {
        var name = p.Name;
        var getterMi = p.GetGetMethod();
        var getterDel = getterMi.MakeInstanceMethodDelegate<Func<TImplem, TProperty>>();
        _fieldGetters[name] = u => getterDel(u);
        
        var setterMi = p.GetSetMethod(true);
        var setterDel = setterMi.MakeInstanceMethodDelegate<Action<TImplem, TProperty>>();
        _fieldSetters[name] = (u, o) =>
        {
            setterDel(u, (TProperty) o);
        };
    }
    public void ForReference()
    {
        return;
        new Meta<TImplem, TMeta>(null);
    }

    public object[] GetArgs(TMeta t)
    {
        var res = new object[_fieldGetters.Count + 1];
        res[0] = typeof(TImplem).Name;
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            res[i + 1] = _fieldGetters[_fieldNames[i]]((TImplem)t);
        }

        return res;
    }

    public TMeta Deserialize(byte[][] argsBytes)
    {
        var args = new object[_fieldNames.Count];
        for (var i = 1; i < argsBytes.Length; i++)
        {
            var fieldName = _fieldNames[i - 1];
            var fieldType = _fieldTypes[fieldName];
            
            if(fieldType != typeof(byte[])) args[i - 1] = Game.I.Serializer.Deserialize(argsBytes[i], fieldType);
            else args[i - 1] = argsBytes[i];
        }
        return _deserializeConstructor(args);
    }
    public void Initialize(TMeta u, object[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            _fieldSetters[_fieldNames[i]]((TImplem)u, args[i]);
        }
    }
}