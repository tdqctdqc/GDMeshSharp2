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
        _container = (Container)FindNode("Container");
        _id = _container.CreateLabelAsChild("Id");
        _numPops = _container.CreateLabelAsChild("Num Pops");
        _regime = _container.CreateLabelAsChild("Regime");
        _landform = _container.CreateLabelAsChild("Landform");
        _veg = _container.CreateLabelAsChild("Veg");
    }
    public void Process(Vector2 mousePosMapSpace)
    {
        var sw = new Stopwatch();
        sw.Start();
        var mouseIn = _client.Data.Cache.MapPolyGrid
            .GetElementAtPoint(mousePosMapSpace, out string msg);
        sw.Stop();
        if (mouseIn is MapPolygon poly)
        {
            if (poly != _mouseOverPoly)
            {
                // GD.Print("time to find " + sw.Elapsed.TotalMilliseconds.ToString());
                // GD.Print(msg);
                var offset = poly.GetOffsetTo(mousePosMapSpace, _client.Data);
            
                _mouseOverPoly = poly;
                Visible = true;
                Position = GetGlobalMousePosition();
                Draw(poly, offset);
                Scale = _client.Cam.Zoom;
                _highlighter.DrawPolyAndNeighbors(poly, _client);
                Visible = true;
            }
        }
        else 
        {
            _mouseOverPoly = null;
            _highlighter.Clear();
            _highlighter.Visible = false;
            Visible = false;
        }
    }

    public void Draw(MapPolygon poly, Vector2 offset)
    {
        _id.Text = "Id: " + poly.Id;
        _numPops.Text = "Num Pops: " + poly.GetNumPeeps(_client.Data);
        _regime.Text = poly.Regime.Empty() ? "Neutral" : poly.Regime.Ref().Name;
        var landform = _client.Data.Models.Landforms.GetAspectAtPoint(poly, offset, _client.Data);
        _landform.Text = "Landform: " + landform.Name;
        var veg = _client.Data.Models.Vegetation.GetAspectAtPoint(poly, offset, _client.Data);
        _veg.Text = "Vegetation: " + veg.Name;
    }
    public void Setup(PolyHighlighter highlighter, IClient client)
    {
        _highlighter = highlighter;
        _client = client;
    }
}