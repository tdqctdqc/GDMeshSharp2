using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DataNotices
{
    public Action FinishedStateSync { get; set; }
    private Dictionary<Type, object> _addedActions;
    //todo danger if entities are derived types? 
    private Dictionary<Type, object> _removingActions;
    private Dictionary<Type, Dictionary<string, object>> _varUpdatedActions;
    
    public DataNotices()
    {
        _addedActions = new Dictionary<Type, object>();
        _removingActions = new Dictionary<Type, object>();
        _varUpdatedActions = new Dictionary<Type, Dictionary<string, object>>();
    }

    public void RegisterEntityAddedCallback<TEntity>(Action<TEntity> callback)
    {
        var eType = typeof(TEntity);
        if (_addedActions.ContainsKey(eType) == false)
        {
            Action<TEntity> a = (TEntity e) => { };
            _addedActions.Add(eType, a);
        }
        var action = ((Action<TEntity>) _addedActions[eType]);
        action += callback;
        _addedActions[eType] = action;
    }
    public void RegisterEntityRemovingCallback<TEntity>(Action<TEntity> callback)
    {
        var eType = typeof(TEntity);
        if (_removingActions.ContainsKey(eType) == false)
        {
            Action<TEntity> a = (TEntity e) => { };
            _removingActions.Add(eType, a);
        }
        var action = ((Action<TEntity>) _removingActions[eType]);
        action += callback;
        _removingActions[eType] = action;
    }
    public void RaiseAddedEntity<TEntity>(TEntity e)
    {
        if (_addedActions.ContainsKey(e.GetType()))
        {
            var action = (Action<TEntity>)_addedActions[e.GetType()];
            action.Invoke(e);
        }
    }
    public void RaiseRemovingEntity<TEntity>(TEntity e)
    {
        if (_removingActions.ContainsKey(e.GetType()))
        {
            var action = (Action<TEntity>)_removingActions[e.GetType()];
            action.Invoke(e);
        }
    }
}

