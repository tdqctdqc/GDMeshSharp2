using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;

public class Meta<TSpecific, TMeta> : IMeta<TMeta> where TSpecific : TMeta
{
    private JsonSerializerOptions _options;
    private List<string> _fieldNames;
    private Dictionary<string, Func<TSpecific, object>> _fieldGetters;
    private Dictionary<string, Action<TSpecific, object>> _fieldSetters;
    private Func<object[], TSpecific> _deserializeConstructor;
    public Meta(JsonSerializerOptions options)
    {
        _options = options;
        _fieldGetters = new Dictionary<string, Func<TSpecific, object>>();
        _fieldSetters = new Dictionary<string, Action<TSpecific, object>>();
        var type = typeof(TSpecific);
        var deserializeMi =
            type.GetMethod("DeserializeConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        var deserializeDel = deserializeMi.MakeInstanceMethodDelegate<Func<object[], TSpecific>>();
        _deserializeConstructor = deserializeDel;

        var properties = type.GetProperties();
        _fieldNames = properties.Select(p => p.Name).ToList();
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
        var getterDel = getterMi.MakeInstanceMethodDelegate<Func<TSpecific, TProperty>>();
        _fieldGetters[name] = u => getterDel(u);
        
        var setterMi = p.GetSetMethod(true);
        var setterDel = setterMi.MakeInstanceMethodDelegate<Action<TSpecific, TProperty>>();
        _fieldSetters[name] = (u, o) => setterDel(u, (TProperty)o);
    }
    public void ForReference()
    {
        return;
        new Meta<TSpecific, TMeta>(null);
    }

    public object[] GetArgs(TMeta update)
    {
        var res = new object[_fieldGetters.Count + 1];
        res[0] = typeof(TSpecific).Name;
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            res[i + 1] = _fieldGetters[_fieldNames[i]]((TSpecific)update);
        }

        return res;
    }

    public TMeta Deserialize(object[] args)
    {
        return _deserializeConstructor(args);
    }
    public void Initialize(TMeta u, object[] args)
    {
        for (var i = 1; i < args.Length; i++)
        {
            _fieldSetters[_fieldNames[i]]((TSpecific)u, args[i]);
        }
    }
}