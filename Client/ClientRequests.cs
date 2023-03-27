using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClientRequests
{
    private EntityTypeTree _tree;
    public RefAction<string> OpenWindowRequest { get; private set; }
    public RefAction<PolyTriPosition> MouseOver { get; private set; }
    public RefAction<ITooltipInstance> PromptTooltip { get; private set; }
    public RefAction<ITooltipInstance> HideTooltip { get; private set; }
    public ClientRequests()
    {
        OpenWindowRequest = new RefAction<string>();
        MouseOver = new RefAction<PolyTriPosition>();
        PromptTooltip = new RefAction<ITooltipInstance>();
        HideTooltip = new RefAction<ITooltipInstance>();
    }
    public void GiveTree(EntityTypeTree tree)
    {
        _tree = tree;
    }

    public void SubscribeForValChange<TEntity, TProperty>(string fieldName, RefAction<ValChangeNotice<TProperty>> callback)
    {
        _tree?[typeof(TEntity)].EntityValChanged.Subscribe<TEntity, TProperty>(fieldName, callback);
    }

    public void SubscribeForValChangeSpecific<TEntity, TProperty>(string fieldName, TEntity t, 
        RefAction<ValChangeNotice<TProperty>> callback)
    {
        _tree?[typeof(TEntity)].EntityValChanged.Subscribe<TEntity, TProperty>(fieldName, callback);
    }

    public void OpenWindow<T>(string name) where T : WindowDialog
    {
        OpenWindowRequest?.Invoke(name);
    }
}
