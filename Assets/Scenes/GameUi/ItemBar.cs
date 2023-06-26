using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemBar : HBoxContainer
{
    public void Setup(Data data)
    {
        AddItem(ItemManager.Food, data);
        AddItem(ItemManager.Recruits, data);
        AddItem(ItemManager.Oil, data);
        AddItem(ItemManager.Iron, data);
        AddItem(ItemManager.IndustrialPoint, data);
        AddItem(ItemManager.FinancialPower, data);
    }

    private void AddItem(Item sr, Data data)
    {
        var display = RegimeItemDisplay.Create(sr, data);
        this.AddChildWithVSeparator(display);
    }
}