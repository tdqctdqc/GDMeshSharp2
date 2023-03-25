using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemBar : HBoxContainer
{
    public void Setup(Data data)
    {
        ValueChangedHandler<Player, EntityRef<Regime>>.RegisterForEntity(nameof(Player.Regime), 
            data.BaseDomain.Players.LocalPlayer, n => SetupForRegime(n.NewVal.Entity(), data)); 
    }
    public void SetupForRegime(Regime r, Data data)
    {
        this.ClearChildren();
        AddItem(ItemManager.Food, r, data);
        AddItem(ItemManager.Recruits, r, data);
        AddItem(ItemManager.Oil, r, data);
        AddItem(ItemManager.Iron, r, data);
    }

    private void AddItem(Item sr, Regime regime, Data data)
    {
        var display = RegimeItemDisplay.Create(sr, regime, data);
        this.AddChildWithVSeparator(display);
    }
}