using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;

public class UpdateMeta<TUpdate> : IUpdateMeta where TUpdate : Update
{
    private JsonSerializerOptions _options;
    private List<string> _fieldNames;
    private Dictionary<string, Func<TUpdate, object>> _fieldGetters;
    private Dictionary<string, Action<TUpdate, object>> _fieldSetters;
    private Func<object[], TUpdate> _deserializeConstructor;
    public UpdateMeta(JsonSerializerOptions options)
    {
        _options = options;
        _fieldGetters = new Dictionary<string, Func<TUpdate, object>>();
        _fieldSetters = new Dictionary<string, Action<TUpdate, object>>();
        var type = typeof(TUpdate);
        var deserializeMi =
            type.GetMethod("DeserializeConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        var deserializeDel = deserializeMi.MakeInstanceMethodDelegate<Func<object[], TUpdate>>();
        _deserializeConstructor = deserializeDel;

        var properties = type.GetProperties();
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
        var getterDel = getterMi.MakeInstanceMethodDelegate<Func<TUpdate, TProperty>>();
        _fieldGetters[name] = u => getterDel(u);
        
        var setterMi = p.GetSetMethod(true);
        var setterDel = setterMi.MakeInstanceMethodDelegate<Action<TUpdate, TProperty>>();
        _fieldSetters[name] = (u, o) => setterDel(u, (TProperty)o);
    }
    public void ForReference()
    {
        return;
        new UpdateMeta<TUpdate>(null);
    }

    public object[] GetArgs(Update update)
    {
        var res = new object[_fieldGetters.Count + 1];
        res[0] = typeof(TUpdate).Name;
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            res[i + 1] = _fieldGetters[_fieldNames[i]]((TUpdate)update);
        }

        return res;
    }

    public Update Deserialize(object[] args)
    {
        return _deserializeConstructor(args);
    }
    public void Initialize(Update u, object[] args)
    {
        for (var i = 1; i < args.Length; i++)
        {
            _fieldSetters[_fieldNames[i]]((TUpdate)u, args[i]);
        }
    }
}