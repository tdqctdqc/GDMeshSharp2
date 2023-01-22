using Godot;
using System;
using System.Collections.Generic;

public class EntityRef<TRef> : IEntityRef where TRef : Entity
{
    public int RefId { get; private set; }
    public TRef Ref() => _ref;
    private TRef _ref;
    
    public static EntityRef<TRef> Construct(TRef entity, CreateWriteKey key)
    {
        var refer = new EntityRef<TRef>(entity.Id);
        refer._ref = entity;
        return refer;
    }
    public EntityRef(int refId)
    {
        //only use for deserialization or inside Construct
        RefId = refId;
    }
    public void SyncRef(ServerWriteKey key)
    {
        try
        {
            _ref = (TRef) key.Data[RefId];
        }
        catch (Exception e)
        {
            GD.Print(key.Data[RefId].GetType().ToString() + " supposed to be " + typeof(TRef).ToString());
            throw;
        }
    }
}
