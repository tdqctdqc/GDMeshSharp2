using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverPolyHandler
{
    private MapPolygon _mouseOverPoly;
    private PolyTri _mouseOverTri;
    private TimerAction _action;
    private DataTooltipInstance<PolyTriPosition> _instance;

    public MouseOverPolyHandler()
    {
        _action = new TimerAction(.05f);
        _instance = new DataTooltipInstance<PolyTriPosition>(new PolyDataTooltipTemplate(), null);
    }
    public void Process(float delta, Data data, Vector2 mousePosMapSpace)
    {
        FindPoly(data, mousePosMapSpace);
    }
    
    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {

        if (mousePosMapSpace.y <= 0f || mousePosMapSpace.y >= data.Planet.Height)
        {
            _mouseOverPoly = null;
            _mouseOverTri = null;
            Game.I.Client.TooltipManager.HideTooltip(_instance);
            return;
        }
        else if (_mouseOverPoly != null && _mouseOverPoly.PointInPoly(mousePosMapSpace, data))
        {
            if (_mouseOverTri != null && _mouseOverTri.ContainsPoint(mousePosMapSpace - _mouseOverPoly.Center))
            {
                return;
            }
        }
        else if (_mouseOverPoly != null && 
                 _mouseOverPoly.Neighbors.Entities()
                         .FirstOrDefault(n => n.PointInPoly(mousePosMapSpace, data))
                     is MapPolygon neighbor)
        {
            _mouseOverPoly = neighbor;
        }
        else
        {
            _mouseOverPoly = data.Planet.Polygons.MapPolyGrid.GetElementAtPoint(mousePosMapSpace);
        }
        FindTri(_mouseOverPoly, data, mousePosMapSpace);
        var pos = new PolyTriPosition(_mouseOverPoly, _mouseOverTri);
        _instance.SetElement(pos);
        Game.I.Client.TooltipManager.PromptTooltip(_instance);
    }
    private void FindTri(MapPolygon p, Data data,  Vector2 mousePosMapSpace)
    {
        var offset = _mouseOverPoly.GetOffsetTo(mousePosMapSpace, data);
        _mouseOverTri = _mouseOverPoly.TerrainTris.GetAtPoint(offset, data);
    }
}
