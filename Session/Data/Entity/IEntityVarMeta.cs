using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IEntityVarMeta<TEntity> where TEntity : Entity
{
    object GetForSerialize(TEntity e);
    void Set(TEntity e, object receivedValue, StrongWriteKey key);
    void Set(TEntity e, object receivedValue, ServerWriteKey key);
    bool Test(TEntity t);
}