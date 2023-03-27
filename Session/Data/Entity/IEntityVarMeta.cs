using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IEntityVarMeta<TEntity> where TEntity : Entity
{
    object GetForSerialize(TEntity e);
    bool Test(TEntity t);
    void UpdateVar(string fieldName, Entity t, StrongWriteKey key, object newValueOb);
}