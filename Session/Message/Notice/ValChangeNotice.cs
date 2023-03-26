using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class ValChangeNotice<TProperty> 
{
    public int EntityId { get; private set; }
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }

    public ValChangeNotice(int entityId, TProperty newVal, TProperty oldVal)
    {
        EntityId = entityId;
        NewVal = newVal;
        OldVal = oldVal;
    }
}