
using System;
using System.Collections.Generic;
using Godot;

public class ValueChangedHandler<TEntity, TProperty> 
    : EntityNoticeHandler<TEntity, ValueChangedNotice<TEntity, TProperty>>
    where TEntity : Entity
{
    private static Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>> _changedAll
        = new Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>>();
    private static Dictionary<int, Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>>> _changedSpecific
        = new Dictionary<int, Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>>>();
  
    public static void RegisterForAll(string fieldName, Action<ValueChangedNotice<TEntity, TProperty>> callback)
    {
        if(_changedAll.ContainsKey(fieldName) == false) _changedAll.Add(fieldName, n => { });
        _changedAll[fieldName] += callback;
    }
    public static void RegisterForEntity(string fieldName, TEntity t, Action<ValueChangedNotice<TEntity, TProperty>> callback)
    {
        if(_changedSpecific.ContainsKey(t.Id) == false) _changedSpecific.Add(t.Id, new Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>>());
        if(_changedSpecific[t.Id].ContainsKey(fieldName) == false) _changedSpecific[t.Id].Add(fieldName, a => { });
        _changedSpecific[t.Id][fieldName] += callback;
    }
    public static void UnregisterForEntity(string fieldName, TEntity t, 
        Action<ValueChangedNotice<TEntity, TProperty>> callback)
    {
        if (_changedSpecific.ContainsKey(t.Id) == false) throw new Exception();
        if (_changedSpecific[t.Id].ContainsKey(fieldName) == false) throw new Exception();
        _changedSpecific[t.Id][fieldName] -= callback;
    }
    public static void UnregisterForAll(string fieldName, Action<ValueChangedNotice<TEntity, TProperty>> callback)
    {
        if (_changedAll.ContainsKey(fieldName) == false) throw new Exception();
        _changedAll[fieldName] -= callback;
    }
    public static void Raise(string valueName,
        TEntity entity, TProperty oldVal, TProperty newVal, WriteKey key)
    {
        var n = new ValueChangedNotice<TEntity, TProperty>(entity, newVal, oldVal);
        Raise(valueName, n);
    }
    protected static void Raise(string fieldName, ValueChangedNotice<TEntity, TProperty> n) 
    {
        if (_changedAll.TryGetValue(fieldName, out var a1))
        {
            a1?.Invoke(n);
        }

        if (_changedSpecific.TryGetValue(n.Entity.Id, out var byFieldName))
        {
            if (byFieldName.TryGetValue(fieldName, out var a2))
            {
                a2?.Invoke(n);
            }
        }
    }
}
