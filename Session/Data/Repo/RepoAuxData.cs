using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class RepoAuxData<TEntity>
    where TEntity : Entity
{
    public abstract void HandleAdded(TEntity added);
    
    public abstract void HandleRemoved(TEntity removing);

    public RepoAuxData(Data data)
    {
        EntityCreatedHandler<TEntity>.Register(n => HandleAdded(n.Entity));
        EntityDestroyedHandler<TEntity>.Register(n => HandleRemoved(n.Entity));
    }
}