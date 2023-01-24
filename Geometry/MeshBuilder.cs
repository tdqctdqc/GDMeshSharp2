using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MeshBuilder
{
    public List<Vector2> TriPoints { get; private set; }
    public List<Color> Colors { get; private set; }
    public Color DefaultColor { get; private set; }

    public MeshBuilder(Color defaultColor)
    {
        DefaultColor = defaultColor;
        TriPoints = new List<Vector2>();
        Colors = new List<Color>();
    }

    private void AddTri(Vector2 a, Vector2 b, Vector2 c, Color? color)
    {
        TriPoints.Add(a);
        TriPoints.Add(b);
        TriPoints.Add(c);
        if(color.HasValue) Colors.Add(color.Value);
        else Colors.Add(DefaultColor);
    }
    
    private void JoinLinePoints(Vector2 from, Vector2 to, float thickness, Color? color = null)
    {
        var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
        var fromOut = from + perpendicular * .5f * thickness;
        var fromIn = from - perpendicular * .5f * thickness;
        var toOut = to + perpendicular * .5f * thickness;
        var toIn = to - perpendicular * .5f * thickness;
        AddTri(fromIn, fromOut, toOut, color);
        AddTri(toIn, toOut, fromIn, color);
    }

    public void AddPolysRelative(MapPolygon relTo, List<MapPolygon> polys, Func<MapPolygon, Color> getColor, Data data)
    {
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var color = getColor(p);
            var polyTriPoints = p.GetTrisRel(data).Select(v => v + relTo.GetOffsetTo(p, data));
            TriPoints.AddRange(polyTriPoints);
            for (int j = 0; j < polyTriPoints.Count() / 3; j++)
            {
                Colors.Add(color);
            }
        }
    }

    public void AddPolyBorders(MapPolygon relTo, List<MapPolygonBorder> borders, float thickness,
        Func<MapPolygon, Color> getColor, Data data)
    {
        foreach (var border in borders)
        {
            var highId = border.HighId.Ref();
            var highColor = getColor(highId);
            
            var lowId = border.LowId.Ref();
            var lowColor = getColor(lowId);
            
            var segs = border.GetSegsRel(highId)
                .ChangeOrigin(highId, relTo, data)
                .ToList();
            
            AddBordersTris(segs, thickness, thickness, highColor, lowColor);
        }
    }
    public void AddBordersTris(List<LineSegment> segs, float inThickness, float outThickness, 
        Color inColor, Color outColor)
    {
        segs.ForEach(seg =>
        {
            var from = seg.From;
            var to = seg.To;
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * outThickness;
            var toOut = to + perpendicular * outThickness;
            var fromIn = from - perpendicular * inThickness;
            var toIn = to - perpendicular * inThickness;
            
            AddTri(fromIn, from, to, inColor);
            AddTri(to, toIn, fromIn, inColor);
            AddTri(fromOut, toOut, from, outColor);
            AddTri(toOut, to, from, outColor);
        });
    }
    public void AddLines(List<Vector2> froms,
        List<Vector2> tos, float thickness, List<Color> colors = null)
    {
        for (int i = 0; i < froms.Count; i++)
        {
            Color? color = null;
            if (colors != null) color = colors[i];
            JoinLinePoints(froms[i], tos[i], thickness, color);
        }
    }
    public void AddLines(List<LineSegment> segs, float thickness, List<Color> colors = null)
    {
        for (int i = 0; i < segs.Count; i++)
        {
            Color? color = null;
            if (colors != null) color = colors[i];
            JoinLinePoints(segs[i].From, segs[i].To, thickness, color);
        }
    }
    public void AddLines(List<LineSegment> segs, float thickness, Color? color = null)
    {
        for (int i = 0; i < segs.Count; i++)
        {
            JoinLinePoints(segs[i].From, segs[i].To, thickness, color);
        }
    }
    public void AddLinesCustomWidths(List<Vector2> froms,
        List<Vector2> tos, List<float> widths, List<Color> colors = null)
    {
        for (int i = 0; i < froms.Count; i++)
        {
            Color? color = null;
            if (colors != null) color = colors[i];
            JoinLinePoints(froms[i], tos[i], widths[i], color);
        }
    }
    
    public void AddCircle(Vector2 center, float radius, int resolution, Color? color = null)
    {
        var angleIncrement = Mathf.Pi * 2f / (float) resolution;
        var triPoints = new List<Vector2>();
        for (int i = 0; i < resolution; i++)
        {
            var startAngle = angleIncrement * i;
            var startPoint = center + Vector2.Up.Rotated(startAngle) * radius;
            var endAngle = startAngle + angleIncrement;
            var endPoint = center + Vector2.Up.Rotated(endAngle) * radius;
            AddTri(center, startPoint, endPoint, color);
        }
    }
    
    public void AddArrow(Vector2 from, Vector2 to, float thickness, Color? color = null)
    {
        var arrow = new Node2D();
        var length = from.DistanceTo(to);
        var lineTo = from + (to - from).Normalized() * length * .8f;
        var perpendicular = (to - from).Normalized().Rotated(Mathf.Pi / 2f);
        JoinLinePoints(from, to, thickness, color);
        AddTri(to, lineTo + perpendicular * thickness,
            lineTo - perpendicular * thickness, color);
    }
    
    public void AddPointMarkers(List<Vector2> points, float markerSize, Color? color = null)
    {
        foreach (var p in points)
        {
            var topLeft = p + Vector2.Up * markerSize / 2f
                            + Vector2.Left * markerSize / 2f;
            var topRight = p + Vector2.Up * markerSize / 2f
                             + Vector2.Right * markerSize / 2f;
            var bottomLeft = p + Vector2.Down * markerSize / 2f
                               + Vector2.Left * markerSize / 2f;
            var bottomRight = p + Vector2.Down * markerSize / 2f
                                + Vector2.Right * markerSize / 2f;
            AddTri(topLeft, topRight, bottomLeft, color);
            AddTri(topRight, bottomRight, bottomLeft, color);
        }
    }
    public MeshInstance2D GetMeshInstance()
    {
        var mesh = MeshGenerator.GetArrayMesh(TriPoints.ToArray(), Colors.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
}