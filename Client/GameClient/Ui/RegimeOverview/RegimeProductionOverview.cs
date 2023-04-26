using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeProductionOverview : ScrollContainer
{
    private VBoxContainer _container;
    public RegimeProductionOverview()
    {
        Name = "Production";

        RectMinSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.RectMinSize = RectMinSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var itemNames = data.Models.Items.Models.Keys;
        
        foreach (var itemName in itemNames)
        {
            var lastProd = regime.History.ProdHistory[itemName].GetLatest();
            var lastDemand = regime.History.DemandHistory[itemName].GetLatest();
            if (lastDemand == 0 && lastProd == 0) continue;
            
            var hbox = new HBoxContainer();
            var item = data.Models.Items.Models[itemName];
            
            hbox.AddChild(item.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"{lastProd} / {lastDemand}");
            _container.AddChild(hbox);
        }
    }
}
