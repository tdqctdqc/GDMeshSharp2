using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeConstructionOverview : ScrollContainer
{
    private VBoxContainer _container;
    public RegimeConstructionOverview()
    {
        Name = "Construction";
        RectMinSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.RectMinSize = RectMinSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var constructions = data.Society.CurrentConstruction
            .ByPoly.Where(kvp => regime.Polygons.RefIds.Contains(kvp.Key))
            .SelectMany(kvp => kvp.Value).ToList();
        
        foreach (var construction in constructions)
        {
            var hbox = new HBoxContainer();
            var building = construction.Model.Model();
            hbox.AddChild(building.Icon.GetTextureRect(Vector2.One * 50f));
            var ticksDone = construction.TicksDone();
            hbox.CreateLabelAsChild($"{ticksDone} / {building.NumTicksToBuild}");
            _container.AddChild(hbox);
        }
    }
}
