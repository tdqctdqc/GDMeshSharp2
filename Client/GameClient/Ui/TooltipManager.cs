using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TooltipManager
{
    private MapPolyTooltip _mapPolyToolTip;
    
    public TooltipManager(MapPolyTooltip mapPolyTooltip, PolyHighlighter higlighter, GameGraphics graphics)
    {
        _mapPolyToolTip = mapPolyTooltip;
        _mapPolyToolTip.Setup(higlighter, graphics);
    }

    public void Process(float delta, Data data, Vector2 mousePosInMapSpace)
    {
        _mapPolyToolTip.Process(delta, data, mousePosInMapSpace);
    }
}