using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class ValueChangedNotice<TEntity, TProperty>
{
    private static Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>> _changed
        = new Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>>();
    public TEntity Entity { get; private set; }
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }
    private static bool _registered = Register();
    
    public ValueChangedNotice(TEntity entity, TProperty newVal, TProperty oldVal)
    {
        Entity = entity;
        NewVal = newVal;
        OldVal = oldVal;
    }

    private static bool Register()
    {
        NoticeMeta.Register(Clear);
        return true;
    }
    private static void Clear()
    {
        _changed = new Dictionary<string, Action<ValueChangedNotice<TEntity, TProperty>>>();
    }

    public static void Raise(string valueName,
        TEntity entity, TProperty oldVal, TProperty newVal, WriteKey key)
    {
        if (_changed.TryGetValue(valueName, out var action))
        {
            var n = new ValueChangedNotice<TEntity, TProperty>(entity, newVal, oldVal);
            action.Invoke(n);
        }
    }
    public static void Register(string fieldName, Action<ValueChangedNotice<TEntity, TProperty>> callback)
    {
        if(_changed.ContainsKey(fieldName) == false) _changed.Add(fieldName, n => { });
        _changed[fieldName] += callback;
    }
    public static void Unregister(string fieldName, Action<ValueChangedNotice<TEntity, TProperty>> callback)
    {
        if (_changed.ContainsKey(fieldName) == false) throw new Exception();
        _changed[fieldName] -= callback;
    }
    
    private static class NoticeMeta
    {
        private static List<Action> _clears = new List<Action>();
        
        static NoticeMeta()
        {
            Game.I.NewSession += Clear;
        }

        private static void Clear()
        {
            _clears.ForEach(c => c?.Invoke());
            _clears = new List<Action>();
        }
        public static void Register(Action clear)
        {
            _clears.Add(clear);
        }
    }
}