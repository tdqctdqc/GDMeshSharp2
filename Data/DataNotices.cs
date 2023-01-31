using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DataNotices
{
    private Dictionary<Type, object> _addedActions;
    private Dictionary<Type, object> _removingActions;
    
    public DataNotices()
    {
        
    }

    public void RegisterEntityAddedCallback<TEntity>(Action<TEntity> callback)
    {
        if (_addedActions.ContainsKey(typeof(TEntity)) == false)
        {
            Action<TEntity> action = (TEntity e) => { };
            _addedActions.Add(typeof(TEntity), action);
        }
    }
    public void RegisterEntityRemovingCallback<TEntity>(Action<TEntity> callback)
    {
        if (_removingActions.ContainsKey(typeof(TEntity)) == false)
        {
            Action<TEntity> action = (TEntity e) => { };
            _removingActions.Add(typeof(TEntity), action);
        }
    }
    public void RaiseAddedEntity<TEntity>(TEntity e)
    {
        if (_addedActions.ContainsKey(e.GetType()))
        {
            var action = (Action<TEntity>)_addedActions[e.GetType()];
            action?.Invoke(e);
        }
    }
    public void RaiseAddedEntity(Entity e, Type entityType)
    {
        if (_addedActions.ContainsKey(e.GetType()))
        {
            var action = (Action<>)_addedActions[e.GetType()];
            action?.Invoke(e);
        }
    }
    public void RaiseRemovingEntity<TEntity>(TEntity e)
    {
        if (_removingActions.ContainsKey(e.GetType()))
        {
            var action = (Action<TEntity>)_removingActions[e.GetType()];
            action?.Invoke(e);
        }
    }
}