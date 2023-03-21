using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolyTooltip : Node2D
{
    private Label _id, _numPeeps, _totalPop, _regime, _landform, _veg, _settlementName, _settlementSize,
        _resourceDeposits, _buildings, _fertility;
    private Container _container;
    private MapPolygon _mouseOverPoly = null;
    private PolyTri _mouseOverTri = null;
    private GameGraphics _graphics;
    private PolyHighlighter _highlighter;
    private TimerAction _action, _detailedAction;
    public void Setup(PolyHighlighter highlighter, GameGraphics graphics)
    {
        _highlighter = highlighter;
        _graphics = graphics;
        this.AssignChildNode(ref _container, "Container");
        _id = _container.CreateLabelAsChild("Id");
        _numPeeps = _container.CreateLabelAsChild("Num Peeps");
        _totalPop = _container.CreateLabelAsChild("TotalPop");
        _regime = _container.CreateLabelAsChild("Regime");
        _landform = _container.CreateLabelAsChild("Landform");
        _veg = _container.CreateLabelAsChild("Veg");
        _settlementName = _container.CreateLabelAsChild("SettlementName");
        _settlementSize = _container.CreateLabelAsChild("SettlementSize");
        _resourceDeposits = _container.CreateLabelAsChild("ResourceDeposits");
        _buildings = _container.CreateLabelAsChild("Buildings");
        _fertility = _container.CreateLabelAsChild("Fertility");
        _action = new TimerAction(.05f);
    }
    public void Process(float delta, Data data, Vector2 mousePosMapSpace)
    {
        _action.ProcessAction(delta, () => FindPoly(data, mousePosMapSpace));
    }
    
    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {
        if (mousePosMapSpace.y <= 0f || mousePosMapSpace.y >= data.Planet.Height)
        {
            _mouseOverPoly = null;
            _mouseOverTri = null;
        }
        else if (_mouseOverPoly != null && _mouseOverPoly.PointInPoly(mousePosMapSpace, data))
        {
        }
        else if (_mouseOverPoly != null && 
                 _mouseOverPoly.Neighbors.Refs()
                         .FirstOrDefault(n => n.PointInPoly(mousePosMapSpace, data))
                 is MapPolygon neighbor)
        {
            SetMousePoly(neighbor);
        }
        else
        {
            SetMousePoly(data.Cache.MapPolyGrid.GetElementAtPoint(mousePosMapSpace));
        }
        PreDraw(mousePosMapSpace, data);
    }

    private void SetMousePoly(MapPolygon p)
    {
        _mouseOverPoly = p;
    }
    private void PreDraw(Vector2 mousePosMapSpace, Data data)
    {
        if (_mouseOverPoly != null)
        {
            var offset = _mouseOverPoly.GetOffsetTo(mousePosMapSpace, data);
            _mouseOverTri = _mouseOverPoly.TerrainTris.GetAtPoint(offset, data);
            Visible = true;
            _highlighter.Draw(data, _mouseOverPoly, _mouseOverTri, offset, _graphics.Client);
            Position = GetGlobalMousePosition() + Vector2.One * 50f;
            DrawTooltip(data, _mouseOverTri, offset);
            Scale = _graphics.Client.Cam.Zoom;
        }
        else 
        {
            _mouseOverPoly = null;
            _mouseOverTri = null;
            _highlighter.Clear();
            _highlighter.Visible = false;
            Visible = false;
        }
    }
    public void DrawTooltip(Data data, PolyTri tri, Vector2 offset)
    {
        _id.Text = "Id: " + _mouseOverPoly.Id;
        var peeps = _mouseOverPoly.GetPeeps(data);
        if (peeps != null)
        {
            _numPeeps.Text = "Num Pops: " + peeps.Count();
        }
        else
        {
            _numPeeps.Text = "";
            _numPeeps.Text = "";
        }
        
        
        _regime.Text = _mouseOverPoly.Regime.Empty() ? "Neutral" : _mouseOverPoly.Regime.Entity().Name;

        if (tri != null)
        {
            _landform.Text = "Landform: " + tri.Landform.Name;
            _veg.Text = "Vegetation: " + tri.Vegetation.Name;
        }

        if (data.Society.Settlements.ByPoly[_mouseOverPoly] is Settlement s)
        {
            _settlementName.Text = "Settlement: " + s.Name;
            _settlementSize.Text = "Settlement size: " + Mathf.FloorToInt(s.Size);
        }
        else
        {
            _settlementName.Text = "";
            _settlementSize.Text = "";
        }
        _resourceDeposits.Text = "";
        var rs = _mouseOverPoly.GetResourceDeposits(data);
        if (rs != null)
        {
            foreach (var r in rs)
            {
                _resourceDeposits.Text += $"{r.Resource.Model().Name}: {Mathf.FloorToInt(r.Size)} \n";
            }
        }
        
        _buildings.Text = "";
        var bs = _mouseOverPoly.GetBuildings(data);
        if (bs != null)
        {
            _buildings.Text = "Buildings: ";
            var counts = bs.Select(b => b.Model.Model()).GetCounts();
            foreach (var kvp in counts)
            {
                _buildings.Text += $"\n\t{kvp.Key.Name} x {kvp.Value}";
            }
        }
        
        _fertility.Text = "Fertility: " + Mathf.FloorToInt(_mouseOverPoly.GetFertility());
    }

}