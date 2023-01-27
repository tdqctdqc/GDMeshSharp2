using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class ConvertibleVarMeta<TEntity, TProp, TBase, TConverted> : EntityVarMeta<TEntity, TProp> 
    where TEntity : Entity
{
    protected Func<TProp, TBase, TConverted> ConvertFromBase { get; private set; }
    protected Func<TProp, TConverted, TBase> ConvertToBase { get; private set; }
    protected Action<TProp, TConverted> SetFromConverted { get; private set; }
    protected Func<TEntity, TConverted> GetConverted { get; private set; }
    public ConvertibleVarMeta(PropertyInfo prop) : base(prop)
    {
        var convertFromBaseMi = prop.PropertyType.GetMethod(nameof(EntityConvertibleVar<int, int>.ConvertFromBase));
        ConvertFromBase = convertFromBaseMi.MakeInstanceMethodDelegate<Func<TProp, TBase, TConverted>>();
        
        var convertToBaseMi = prop.PropertyType.GetMethod(nameof(EntityConvertibleVar<int, int>.ConvertToBase));
        ConvertToBase = convertToBaseMi.MakeInstanceMethodDelegate<Func<TProp, TConverted, TBase>>();

        var setMi = prop.PropertyType.GetProperty(nameof(EntityConvertibleVar<int, int>.Value)).GetSetMethod(true);
        
        SetFromConverted = setMi.MakeInstanceMethodDelegate<Action<TProp, TConverted>>();
        
        var getMi = prop.PropertyType.GetProperty(nameof(EntityConvertibleVar<int, int>.Value)).GetGetMethod();
        var getMiDel = getMi.MakeInstanceMethodDelegate<Func<TProp, TConverted>>();
        GetConverted = e => getMiDel(GetProperty(e));
    }
    
    public override void Set(TEntity e, object receivedValue, ServerWriteKey key)
    {
        var prop = GetProperty(e);
        var baseValue = (TBase) receivedValue;
        var convertedValue = ConvertFromBase(prop, baseValue);
        SetFromConverted(prop, convertedValue);
    }
    public override void Set(TEntity e, object receivedValue, CreateWriteKey key)
    {
        var prop = GetProperty(e);
        var convertedValue = (TConverted) receivedValue;
        SetFromConverted(prop, convertedValue);
    }
    public override object GetForSerialize(TEntity e)
    {
        return (TBase) ConvertToBase(GetProperty(e), GetConverted(e));
    }
    public override object GetFromSerialized(byte[] bytes)
    {
        return Game.I.Serializer.Deserialize<TBase>(bytes);
    }
}