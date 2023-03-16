using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolyTooltip : Node2D
{
    private Label _id, _numPops, _regime, _landform, _veg, _settlementName, _settlementSize,
        _resourceDeposits;
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
        _resourceDeposits = _container.CreateLabelAsChild("ResourceDeposits");
        _action = new TimerAction(.05f);
    }
    public void Process(float delta, Data data, Vector2 mousePosMapSpace)
    {
        _action.ProcessVariableFunc(delta, () => FindPoly(data, mousePosMapSpace));
    }
    
    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {
        if (mousePosMapSpace.y <= 0f || mousePosMapSpace.y >= data.Planet.Height)
        {
            _mouseOverPoly = null;
        }
        else if (_mouseOverPoly != null && _mouseOverPoly.PointInPoly(mousePosMapSpace, data))
        {
        }
        else if (_mouseOverPoly != null && 
                 _mouseOverPoly.Neighbors.Refs().FirstOrDefault(n => n.PointInPoly(mousePosMapSpace, data))
                 is MapPolygon neighbor)
        {
            _mouseOverPoly = neighbor;
        }
        else
        {
            _mouseOverPoly = data.Cache.MapPolyGrid
                .GetElementAtPoint(mousePosMapSpace);
        }
        PreDraw(mousePosMapSpace, data);
    }

    private void PreDraw(Vector2 mousePosMapSpace, Data data)
    {
        if (_mouseOverPoly != null)
        {
            var offset = _mouseOverPoly.GetOffsetTo(mousePosMapSpace, data);
            var tri = _mouseOverPoly.TerrainTris.GetAtPoint(offset, data);
            Visible = true;
            _highlighter.Draw(data, _mouseOverPoly, tri, offset, _client);
            Position = GetGlobalMousePosition() + Vector2.One * 20f;
            DrawTooltip(data, tri, offset);
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
    public void DrawTooltip(Data data, PolyTri tri, Vector2 offset)
    {
        _id.Text = "Id: " + _mouseOverPoly.Id;
        _numPops.Text = "Num Pops: " + _mouseOverPoly.GetNumPeeps(data);
        _regime.Text = _mouseOverPoly.Regime.Empty() ? "Neutral" : _mouseOverPoly.Regime.Entity().Name;

        if (tri != null)
        {
            _landform.Text = "Landform: " + tri.Landform.Name;
            _veg.Text = "Vegetation: " + tri.Vegetation.Name;
        }

        if (data.Society.Settlements.ByPoly[_mouseOverPoly] is Settlement s)
        {
            _settlementName.Text = "Settlement: " + s.Name;
            _settlementSize.Text = "Settlement size: " + s.Size.ToString();
        }
        else
        {
            _settlementName.Text = "";
            _settlementSize.Text = "";
        }

        _resourceDeposits.Text = "";
        var rs = _mouseOverPoly.GetResourceDeposits(data);
        if (rs == null) return;
        foreach (var r in rs)
        {
            _resourceDeposits.Text += $"{r.Resource.Model().Name}: {r.Size} \n";
        }
    }
    
}