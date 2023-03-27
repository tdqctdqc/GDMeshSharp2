using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class ValChangeNotice<TProperty> : IEntityNotice
{
    Type IEntityNotice.EntityType => Entity.GetType();
    public Entity Entity { get; private set; }
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }
    public ValChangeNotice(Entity entity, TProperty newVal, TProperty oldVal)
    {
        Entity = entity;
        NewVal = newVal;
        OldVal = oldVal;
    }
}