using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ValueChangedNotice<TEntity, TProperty>
{
    public TEntity Entity { get; private set; }
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }

    public ValueChangedNotice(TEntity entity, TProperty newVal, TProperty oldVal)
    {
        Entity = entity;
        NewVal = newVal;
        OldVal = oldVal;
    }
}