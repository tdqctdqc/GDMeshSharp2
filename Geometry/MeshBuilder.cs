using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MeshBuilder
{
    public List<Triangle> Tris { get; private set; }
    public List<Color> Colors { get; private set; }
    public List<Label> Labels { get; private set; }

    public MeshBuilder()
    {
        Tris = new List<Triangle>();
        Colors = new List<Color>();
        Labels = new List<Label>();
    }

    public void Clear()
    {
        Tris.Clear();
        Colors.Clear();
    }
    public void AddTriOutline(Triangle tri, float thickness, Color color)
    {
        var center = tri.GetCentroid();
        var aIn = center + (tri.A - center).Normalized() * ((tri.A - center).Length() - thickness);
        var bIn = center + (tri.B - center).Normalized() * ((tri.B - center).Length() - thickness);
        var cIn = center + (tri.C - center).Normalized() * ((tri.C - center).Length() - thickness);
        
        AddTri(tri.A, aIn, tri.B, color);
        AddTri(tri.B, bIn, tri.A, color);
        
        AddTri(tri.A, aIn, tri.C, color);
        AddTri(tri.C, cIn, tri.A, color);
        
        AddTri(tri.B, bIn, tri.C, color);
        AddTri(tri.C, cIn, tri.B, color);
    }
    public void AddTri(Triangle tri, Color color)
    {
        Tris.Add(tri);
        Colors.Add(color);
    }
    
    public void AddTri(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        var tri = new Triangle(a, b, c);
        Tris.Add(tri);
        Colors.Add(color);
    }
    
    private void JoinLinePoints(Vector2 from, Vector2 to, float thickness, Color color)
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
            var polyTris = data.Cache.PolyRelWheelTris[p].Select(v => v.Transpose(relTo.GetOffsetTo(p, data))).ToList();
            for (int j = 0; j < polyTris.Count(); j++)
            {
                AddTri(polyTris[j], color);
            }
        }
    }
    public void AddPolyWheelTrisRelative(MapPolygon relTo, List<MapPolygon> polys, 
        Func<int, Color> getColor, Data data)
    {
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var tris = p.BorderSegments.Select(b => new Triangle(b.From, b.To, Vector2.Zero));
            var polyTris = tris.Select(v => v.Transpose(relTo.GetOffsetTo(p, data))).ToList();
            for (int j = 0; j < polyTris.Count(); j++)
            {
                var color = getColor(j);
                AddTri(polyTris[j], color);
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
        List<Vector2> tos, float thickness, List<Color> colors)
    {
        for (int i = 0; i < froms.Count; i++)
        {
            var color = colors[i];
            JoinLinePoints(froms[i], tos[i], thickness, color);
        }
    }
    public void AddLines(List<LineSegment> segs, float thickness, List<Color> colors)
    {
        for (int i = 0; i < segs.Count; i++)
        {
            var color = colors[i];
            JoinLinePoints(segs[i].From, segs[i].To, thickness, color);
        }
    }
    public void AddLines(List<LineSegment> segs, float thickness, Color color)
    {
        for (int i = 0; i < segs.Count; i++)
        {
            JoinLinePoints(segs[i].From, segs[i].To, thickness, color);
        }
    }
    public void AddLinesCustomWidths(List<Vector2> froms,
        List<Vector2> tos, List<float> widths, List<Color> colors)
    {
        for (int i = 0; i < froms.Count; i++)
        {
            var color = colors[i];
            JoinLinePoints(froms[i], tos[i], widths[i], color);
        }
    }
    
    public void AddCircle(Vector2 center, float radius, int resolution, Color color)
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

    public void AddArrowsRainbow(List<LineSegment> segs, float thickness)
    {
        for (var i = 0; i < segs.Count; i++)
        {
            AddArrow(segs[i].From, segs[i].To, thickness, ColorsExt.GetRainbowColor(i));
        }
    }
    public void AddArrows(List<LineSegment> segs, float thickness, Color color)
    {
        segs.ForEach(s => AddArrow(s.From, s.To, thickness, color));
    }
    public void AddArrow(Vector2 from, Vector2 to, float thickness, Color color)
    {
        var length = from.DistanceTo(to);
        
        // var lineTo = from + (to - from).Normalized() * (length - thickness * 2f);
        var mid = (from + to) / 2f;
        var arrowTo = from + (to - from) / 1.75f;
        var lineTo = to;
        
        var perpendicular = (to - from).Normalized().Rotated(Mathf.Pi / 2f);
        JoinLinePoints(from, lineTo, thickness, color);
        AddTri(arrowTo, mid + perpendicular * thickness * 2f,
            mid - perpendicular * thickness * 2f, color);
    }

    public void AddNumMarkers(List<Vector2> points, float markerSize, Color color)
    {
        AddPointMarkers(points, markerSize, color);
        for (var i = 0; i < points.Count; i++)
        {
            var label = new Label();
            label.Text = i.ToString();
            label.RectPosition = points[i];
            Labels.Add(label);
        }
    }
    public void AddPointMarkers(List<Vector2> points, float markerSize, Color color)
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
        var mesh = MeshGenerator.GetArrayMesh(Tris.GetTriPoints().ToArray(), Colors.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        Labels.ForEach(l => meshInstance.AddChild(l));
        return meshInstance;
    }
}