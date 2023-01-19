using Godot;
using System;
using System.Collections.Generic;

public class EntityRef<TRef> where TRef : Entity
{
    public int RefId { get; private set; }
    public TRef Ref(Data data) => (TRef) data[RefId];
    public EntityRef(int refId)
    {
        RefId = refId;
    }
}
