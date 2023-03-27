using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemBar : HBoxContainer
{
    private RefAction<ValChangeNotice<EntityRef<Regime>>> _setupForRegime;
    public void Setup(Data data)
    {
        _setupForRegime = new RefAction<ValChangeNotice<EntityRef<Regime>>>();
        _setupForRegime.Subscribe(n => SetupForRegime(n.NewVal.Entity(), data));
        
        Game.I.Client.Requests.SubscribeForValChangeSpecific<Player, EntityRef<Regime>>(
            nameof(Player.Regime), 
            data.BaseDomain.PlayerAux.LocalPlayer, 
            _setupForRegime
        ); 
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

    public override void _ExitTree()
    {
        _setupForRegime.EndSubscriptions();
    }
}