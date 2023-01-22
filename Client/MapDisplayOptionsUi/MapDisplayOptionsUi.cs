using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapDisplayOptionsUi : Container
{
    public static MapDisplayOptionsUi Get() 
        => (MapDisplayOptionsUi) ((PackedScene) GD.Load("res://Client/MapDisplayOptionsUi/MapDisplayOptionsUi.tscn")).Instance();
    private ButtonToken _roads, _regimes, _landforms, _vegetation;
    public override void _Ready()
    {
        
    }

    public void Setup(GameGraphics graphics)
    {
        _roads = ButtonToken.Get(this, "Roads", () =>
        {
            Toggle(graphics.MapChunks, r => r.ToggleRoads(), _roads, "Roads");
        });
        _regimes = ButtonToken.Get(this, "Regimes", () =>
        {
            Toggle(graphics.MapChunks, r => r.ToggleRegimes(), _regimes, "Regimes");
        });
        _landforms = ButtonToken.Get(this, "Landforms", () =>
        {
            Toggle(graphics.MapChunks, r => r.ToggleLandforms(), _landforms, "Landforms");

        });
        _vegetation = ButtonToken.Get(this, "Vegetation", () =>
        {
            Toggle(graphics.MapChunks, r => r.ToggleVegetation(), _vegetation, "Vegetation");
        });
    }
    private void Toggle(IEnumerable<MapChunkGraphic> chunks, Func<MapChunkGraphic, bool> toggle, ButtonToken token, string name)
    {
        bool vis = false;
        foreach (var t in chunks)
        {
            vis = toggle(t);
        }

        token.Button.Text = vis
            ? "Showing " + name
            : name + " is hidden";
    }
}