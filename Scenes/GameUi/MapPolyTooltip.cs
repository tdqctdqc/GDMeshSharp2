using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolyTooltip : Node2D
{
    private Label _id, _numPops, _regime, _landform, _veg, _settlementName, _settlementSize;
    private Container _container;
    private MapPolygon _mouseOverPoly = null;
    private IClient _client;
    private PolyHighlighter _highlighter;
    private TimerAction _action;
    public void Setup(PolyHighlighter highlighter, IClient client)
    {
        _highlighter = highlighter;
        _client = client;
        this.AssignChildNode(ref _container, "Container");
        _id = _container.CreateLabelAsChild("Id");
        _numPops = _container.CreateLabelAsChild("Num Pops");
        _regime = _container.CreateLabelAsChild("Regime");
        _landform = _container.CreateLabelAsChild("Landform");
        _veg = _container.CreateLabelAsChild("Veg");
        _settlementName = _container.CreateLabelAsChild("SettlementName");
        _settlementSize = _container.CreateLabelAsChild("SettlementSize");
        _action = new TimerAction(.1f, .1f);
    }
    public void Process(float delta, Data data, Vector2 mousePosMapSpace)
    {
        _action.ProcessVariableFunc(delta, () => FindPoly(data, mousePosMapSpace));
    }

    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {
        MapPolygon mouseIn;
        if (mousePosMapSpace.y <= 0f || mousePosMapSpace.y >= data.Planet.Height)
        {
            mouseIn = null;
        }
        else if (_mouseOverPoly != null && _mouseOverPoly.PointInPoly(mousePosMapSpace, data))
        {
            mouseIn = _mouseOverPoly;
        }
        else
        {
            mouseIn = data.Cache.MapPolyGrid
                .GetElementAtPoint(mousePosMapSpace);
        }
        
        if (mouseIn is MapPolygon poly)
        {
            var offset = poly.GetOffsetTo(mousePosMapSpace, data);
            var tri = poly.TerrainTris.GetAtPoint(offset, data);
            if (poly != _mouseOverPoly)
            {
                _mouseOverPoly = poly;
                Visible = true;
            }
            _highlighter.Draw(data, poly, tri, offset, _client);

            Position = GetGlobalMousePosition() + Vector2.One * 20f;
            Draw(data, poly, offset);
            Scale = _client.Cam.Zoom;
        }
        else 
        {
            _mouseOverPoly = null;
            _highlighter.Clear();
            _highlighter.Visible = false;
            Visible = false;
        }
    }

    public void Draw(Data data, MapPolygon poly, Vector2 offset)
    {
        _id.Text = "Id: " + poly.Id;
        _numPops.Text = "Num Pops: " + poly.GetNumPeeps(data);
        _regime.Text = poly.Regime.Empty() ? "Neutral" : poly.Regime.Entity().Name;

        var tri = poly.TerrainTris.GetAtPoint(offset, data);
        if (tri != null)
        {
            _landform.Text = "Landform: " + tri.Landform.Name;
            _veg.Text = "Vegetation: " + tri.Vegetation.Name;
        }

        if (data.Society.Settlements.ByPoly[poly] is Settlement s)
        {
            _settlementName.Text = "Settlement: " + s.Name;
            _settlementSize.Text = "Settlement size: " + s.Size.ToString();
        }
        else
        {
            _settlementName.Text = "";
            _settlementSize.Text = "";
        }
    }
    
}