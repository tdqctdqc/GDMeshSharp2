using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsExceptionDisplay : Node2D
{
    public void Setup(SegmentsException e)
    {
        
        var widthStep = .3f;
        var markerSize = 1f;
        int iter = 0;
        var pos = Vector2.Zero;
        var buffer = Vector2.Down * 100f;
        
        var message = new Label();
        message.Text = e.Message;
        message.RectScale = Vector2.One * 2f;
        message.RectPosition = -buffer * 2f;
        AddChild(message);
        for (int i = 0; i < e.SegLayers.Count; i++)
        {
            var mb = new MeshBuilder();
            
            mb.AddCircle(Vector2.Zero, 10f, 12, Colors.Pink);
            var segs = e.SegLayers[i];
            var height = segs.Max(s => Mathf.Max(s.From.y, s.To.y)) - segs.Min(s => Mathf.Min(s.From.y, s.To.y));
            var width = segs.Max(s => Mathf.Max(s.From.x, s.To.x)) - segs.Min(s => Mathf.Min(s.From.x, s.To.x));
            
            
            var col = ColorsExt.GetRainbowColor(iter);
            mb.AddArrows(segs, (e.SegLayers.Count - iter + 1) * widthStep, col);
            mb.AddNumMarkers(segs.Select(s => s.Mid()).ToList(), markerSize, Colors.Transparent, Colors.White, Vector2.Zero);
            var layerLabel = new Label();
            layerLabel.Text = "Segment " + e.SegLayerNames[i];
            layerLabel.SelfModulate = col;
            AddChild(layerLabel);
            layerLabel.RectPosition = pos;
            layerLabel.RectPosition -= buffer / 2f;
            iter++;

            var segsLabels = new VBoxContainer();
            segsLabels.RectPosition = pos + width * Vector2.Right;
            AddChild(segsLabels);
            for (var j = 0; j < segs.Count; j++)
            {
                var segInfo = new Label();
                segInfo.Text = j + " " + segs[j].ToString();
                segInfo.Text += " clockwise " + segs[j].IsClockwise(Vector2.Zero); 
                segsLabels.AddChild(segInfo);
            }
            
            
            for (var j = 0; j < segs.Count; j++)
            {
                var seg = segs[j];
                if (seg.To == segs.Modulo(j + 1).From)
                {
                    mb.AddPointMarker(seg.To, markerSize, Colors.White);
                }
                else
                {
                    mb.AddPointMarker(seg.To, markerSize, Colors.Black);
                    mb.AddPointMarker(segs.Modulo(j + 1).From, markerSize, Colors.Red);
                }
            }
            
            

            var child = mb.GetMeshInstance();
            child.Position = pos;
            AddChild(child);
            pos += height * Vector2.Down;
            pos += buffer;
        }
        for (var i = 0; i < e.PointSets.Count; i++)
        {
            var mb = new MeshBuilder();

            var points = e.PointSets[i];
            var col = ColorsExt.GetRainbowColor(iter);
            mb.AddPointMarkers(points, markerSize, col);
            var width = points.Max(s => s.x) - points.Min(s => s.x);

            
            var pointssLabels = new VBoxContainer();
            pointssLabels.RectPosition = pos + width * Vector2.Right;
            AddChild(pointssLabels);
            for (var j = 0; j < points.Count; j++)
            {
                var pointInfo = new Label();
                pointInfo.Text = j + " " + points[j].ToString();
                pointssLabels.AddChild(pointInfo);
            }
            
            mb.AddNumMarkers(points, markerSize, Colors.Transparent, Colors.White,
                Vector2.Zero);
            
            var label = new Label();
            label.Text = "Points " + e.PointSetNames[i];
            label.SelfModulate = col;
            AddChild(label);
            label.RectPosition = pos;
            label.RectPosition -= buffer / 2f;
            iter++;
            var child = mb.GetMeshInstance();
            child.Position = pos;
            AddChild(child);

            var height = points.Max(p => p.y) - points.Min(p => p.y);
            pos += Vector2.Down * height;
            pos += buffer;
        }
    }

    
}