using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class MapDisplayOptionsUi : Container
{
    private ButtonToken _roads, _regimes, _landforms, _vegetation;
    public override void _Ready()
    {
        
    }

    public void Setup(GameGraphics graphics)
    {
        var mapChunkGraphicType = typeof(MapChunkGraphic);
        var toggleable = mapChunkGraphicType
            .GetProperties()
            .Where(p => p.HasAttribute<ToggleableAttribute>());
        
        foreach (var pi in toggleable)
        {
            var name = pi.Name;
            var btn = new Button();
            btn.Text = "Showing " + name;
            Action toggle = () =>
            {
                foreach (var mc in graphics.MapChunkGraphics)
                {
                    var n = (Node2D) pi.GetMethod.Invoke(mc, null);
                    Toggle(mc, n, btn, name);
                }
            };
            
            var token = ButtonToken.Get(btn, toggle);
            AddChild(btn);
        }
    }
    private void Toggle(MapChunkGraphic mc, Node2D n,  Button btn, string name)
    {
        bool vis = n.Toggle();

        btn.Text = vis
            ? "Showing " + name
            : name + " is hidden";
    }
}