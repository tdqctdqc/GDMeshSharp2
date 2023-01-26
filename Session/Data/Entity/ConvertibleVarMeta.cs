using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class ConvertibleVarMeta<TEntity, TProp, TBase, TConverted> : EntityVarMeta<TEntity, TProp> 
    where TEntity : Entity
{
    protected Func<TBase, TConverted> ConvertFromBase { get; private set; }
    protected Func<TConverted, TBase> ConvertToBase { get; private set; }
    protected Action<TProp, TConverted> SetFromConverted { get; private set; }
    public ConvertibleVarMeta(PropertyInfo prop) : base(prop)
    {
        var convertFromBaseMi = prop.PropertyType.GetMethod(nameof(EntityConvertibleVar<int, int>.ConvertFromBase));
        ConvertFromBase = convertFromBaseMi.MakeInstanceMethodDelegate<Func<TBase, TConverted>>();
        
        var convertToBaseMi = prop.PropertyType.GetMethod(nameof(EntityConvertibleVar<int, int>.ConvertToBase));
        ConvertToBase = convertFromBaseMi.MakeInstanceMethodDelegate<Func<TConverted, TBase>>();

        var setMi = prop.PropertyType.GetProperty(nameof(EntityConvertibleVar<int, int>.Value)).GetSetMethod(true);
        SetFromConverted = setMi.MakeInstanceMethodDelegate<Action<TProp, TConverted>>();
    }
    
    public override void Set(TEntity e, object receivedValue, ServerWriteKey key)
    {
        var prop = FieldGetter(e);
        var baseValue = (TBase) receivedValue;
        var convertedValue = ConvertFromBase(baseValue);
        SetFromConverted(prop, convertedValue);
    }
    public override void Set(TEntity e, object receivedValue, CreateWriteKey key)
    {
        var prop = FieldGetter(e);
        var convertedValue = (TConverted) receivedValue;
        SetFromConverted(prop, convertedValue);
    }
}