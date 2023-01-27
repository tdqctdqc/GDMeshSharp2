using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class EntityConvertibleVar<TBase, TConverted>
{
    public abstract TConverted ConvertFromBase(TBase b);
    public abstract TBase ConvertToBase(TConverted c);

    public TBase GetBase()
    {
        return ConvertToBase(Value);
    }

    public TConverted Value { get; protected set; }
    protected EntityConvertibleVar(TConverted c, CreateWriteKey key)
    {
        Value = c;
    }
    protected EntityConvertibleVar(TBase b)
    {
        Value = ConvertFromBase(b);
    }
}

public class ConvertibleAttribute : Attribute
{
    
}