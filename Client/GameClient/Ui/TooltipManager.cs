using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TooltipManager
{
    private IClient _client;
    private MapPolyTooltip _mapPolyToolTip;
    public TooltipManager(MapPolyTooltip mapPolyTooltip, PolyHighlighter higlighter, IClient client)
    {
        _mapPolyToolTip = mapPolyTooltip;
        _client = client;
        _mapPolyToolTip.Setup(higlighter, client);
    }

    public void Process(float delta, Data data, Vector2 mousePosInMapSpace)
    {
        _mapPolyToolTip.Process(delta, data, mousePosInMapSpace);
    }
}