using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

public class ProcedureMeta<TProc> : IProcedureMeta where TProc : Procedure
{
    private JsonSerializerOptions _options;
    private List<string> _fieldNames;
    private Dictionary<string, Func<TProc, object>> _fieldGetters;
    private Dictionary<string, Action<TProc, object>> _fieldSetters;
    private Func<object[], TProc> _deserializeConstructor;
    public ProcedureMeta(JsonSerializerOptions options)
    {
        _options = options;
        _fieldGetters = new Dictionary<string, Func<TProc, object>>();
        _fieldSetters = new Dictionary<string, Action<TProc, object>>();
        var type = typeof(TProc);
        var deserializeMi =
            type.GetMethod("DeserializeConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        var deserializeDel = deserializeMi.MakeInstanceMethodDelegate<Func<object[], TProc>>();
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
        var getterDel = getterMi.MakeInstanceMethodDelegate<Func<TProc, TProperty>>();
        _fieldGetters[name] = u => getterDel(u);
        
        var setterMi = p.GetSetMethod(true);
        var setterDel = setterMi.MakeInstanceMethodDelegate<Action<TProc, TProperty>>();
        _fieldSetters[name] = (u, o) => setterDel(u, (TProperty)o);
    }
    public void ForReference()
    {
        return;
        new ProcedureMeta<TProc>(null);
    }

    public object[] GetArgs(Procedure update)
    {
        var res = new object[_fieldGetters.Count + 1];
        res[0] = typeof(TProc).Name;
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            res[i + 1] = _fieldGetters[_fieldNames[i]]((TProc)update);
        }

        return res;
    }

    public Procedure Deserialize(object[] args)
    {
        return _deserializeConstructor(args);
    }
    public void Initialize(Procedure u, object[] args)
    {
        for (var i = 1; i < args.Length; i++)
        {
            _fieldSetters[_fieldNames[i]]((TProc)u, args[i]);
        }
    }
}
