using Godot;
using System;
using System.Collections.Generic;

public class EntityRef<TRef> where TRef : Entity
{
    public int RefId { get; private set; }
    public TRef Ref => _ref;
    private TRef _ref;

    public EntityRef(TRef entity)
    {
        RefId = entity.Id;
        _ref = entity;
    }
    public EntityRef(int refId, Data data)
    {
        RefId = refId;
        try
        {
            _ref = (TRef) data[RefId];
        }
        catch (Exception e)
        {
            GD.Print(data[RefId].GetType().ToString() + " supposed to be " + typeof(TRef).ToString());
            throw;
        }
    }
}
