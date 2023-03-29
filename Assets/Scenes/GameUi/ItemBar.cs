using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemBar : HBoxContainer
{
    private RefAction _draw;
    public void Setup(Data data)
    {
        AddItem(ItemManager.Food, data);
        AddItem(ItemManager.Recruits, data);
        AddItem(ItemManager.Oil, data);
        AddItem(ItemManager.Iron, data);
    }

    private void AddItem(Item sr, Data data)
    {
        var display = RegimeItemDisplay.Create(sr, data);
        this.AddChildWithVSeparator(display);
    }

    public override void _ExitTree()
    {
        _draw.EndSubscriptions();
    }
}