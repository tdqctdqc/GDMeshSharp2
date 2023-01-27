using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IEntityVarMeta<TEntity> where TEntity : Entity
{
    object GetForSerialize(TEntity e);
    object GetFromSerialized(byte[] bytes);
    void Set(TEntity e, object receivedValue, CreateWriteKey key);
    void Set(TEntity e, object receivedValue, ServerWriteKey key);
}