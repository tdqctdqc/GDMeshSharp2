using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ResourceBar : HBoxContainer
{
    public void Setup(Data data)
    {
        ValueChangedHandler<Player, EntityRef<Regime>>.RegisterForEntity(nameof(Player.Regime), 
            data.BaseDomain.Players.LocalPlayer, n => SetupForRegime(n.NewVal.Entity(), data)); 
    }
    public void SetupForRegime(Regime r, Data data)
    {
        this.ClearChildren();
        AddStratResource(ItemManager.Food, r, data);
        AddStratResource(ItemManager.Recruits, r, data);
        AddStratResource(ItemManager.Fuel, r, data);
        AddStratResource(ItemManager.Iron, r, data);
    }

    private void AddStratResource(Item sr, Regime regime, Data data)
    {
        var display = RegimeResourceDisplay.Create(sr, regime, data);
        this.AddChildWithVSeparator(display);
    }
}