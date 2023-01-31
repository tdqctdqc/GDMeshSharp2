using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class RepoAuxData<TEntity>
    where TEntity : Entity
{
    public abstract void HandleAdded(TEntity added, WriteKey key);
    public abstract void HandleRemoved(TEntity removing, WriteKey key);

    public RepoAuxData(Repository<TEntity> repo)
    {
        repo.AddedEntity += HandleAdded;
        repo.RemovingEntity += HandleRemoved;
    }
}