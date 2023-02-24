using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolyTooltip : Node2D
{
    private Label _id, _numPops, _regime, _landform, _veg;
    private Container _container;
    private MapPolygon _mouseOverPoly = null;
    private IClient _client;
    private PolyHighlighter _highlighter;
    public override void _Ready()
    {
        this.AssignChildNode(ref _container, "Container");
        _id = _container.CreateLabelAsChild("Id");
        _numPops = _container.CreateLabelAsChild("Num Pops");
        _regime = _container.CreateLabelAsChild("Regime");
        _landform = _container.CreateLabelAsChild("Landform");
        _veg = _container.CreateLabelAsChild("Veg");
    }
    public void Process(Data data, Vector2 mousePosMapSpace)
    {
        var mouseIn = data.Cache.MapPolyGrid
            .GetElementAtPoint(mousePosMapSpace);
        if (mouseIn is MapPolygon poly)
        {
            var offset = poly.GetOffsetTo(mousePosMapSpace, data);
            if (poly != _mouseOverPoly)
            {
                _mouseOverPoly = poly;
                Visible = true;
                _highlighter.DrawOutline(data, poly, offset, _client);
            }
            
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
        _regime.Text = poly.Regime.Empty() ? "Neutral" : poly.Regime.Ref().Name;

        var tri = poly.TerrainTris.GetAtPoint(offset, data);
        if (tri != null)
        {
            _landform.Text = "Landform: " + tri.Landform.Name;
            _veg.Text = "Vegetation: " + tri.Vegetation.Name;
        }
    }
    public void Setup(PolyHighlighter highlighter, IClient client)
    {
        _highlighter = highlighter;
        _client = client;
    }
}