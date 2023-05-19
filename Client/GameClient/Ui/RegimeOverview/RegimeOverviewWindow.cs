using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeOverviewWindow : WindowDialog
{
    private TabContainer _container;
    private RegimeConstructionOverview _construction;
    private RegimePeepsOverview _peeps;
    private RegimeProductionOverview _prod;
    private RegimeWalletOverview _wallet;
    private VBoxContainer _regimeTemplates;
    public RegimeOverviewWindow()
    {
        RectSize = new Vector2(500f, 500f);
        _container = new TabContainer();
        _container.RectMinSize = RectSize;
        
        _construction = new RegimeConstructionOverview();
        _container.AddChild(_construction);

        _peeps = new RegimePeepsOverview();
        _container.AddChild(_peeps);

        _prod = new RegimeProductionOverview();
        _container.AddChild(_prod);

        _wallet = new RegimeWalletOverview();
        _container.AddChild(_wallet);
        AddChild(_container);


        var scroll = new ScrollContainer();
        scroll.Name = "Regime Templates";
        scroll.RectSize = new Vector2(200f, 400f);
        _regimeTemplates = new VBoxContainer();
        _regimeTemplates.RectSize = new Vector2(200f, 400f);
        scroll.AddChild(_regimeTemplates);
        _container.AddChild(scroll);
        
    }
    public void Setup(Regime regime, Data data)
    {
        _construction.Setup(regime, data);
        _peeps.Setup(regime, data);
        _prod.Setup(regime, data);
        _wallet.Setup(regime, data);
        _regimeTemplates.ClearChildren();
        foreach (var kvp in data.Models.Cultures.Models)
        {
            kvp.Value.RegimeTemplates.ForEach(rt =>
            {
                var entry = new HBoxContainer();
                
                var colBox = new VBoxContainer();
                var primRect = new ColorRect();
                primRect.Color = new Color(rt.PrimaryColor);
                primRect.RectMinSize = new Vector2(10f, 50f);
                var secRect = new ColorRect();
                secRect.Color = new Color(rt.SecondaryColor);
                secRect.RectMinSize = new Vector2(10f, 50f);
                colBox.AddChild(primRect);
                colBox.AddChild(secRect);
                entry.AddChild(colBox);
                
                var flagRect = new TextureRect();
                flagRect.Expand = true;
                flagRect.RectMinSize = new Vector2(150f, 100f);
                flagRect.Texture = rt.Flag;
                entry.AddChild(flagRect);
                var l = new Label();
                l.Text = rt.Name;
                entry.AddChild(l);
                _regimeTemplates.AddChild(entry);
            });
        }
    }
}