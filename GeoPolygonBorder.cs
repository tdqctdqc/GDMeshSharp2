using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeoPolygonBorder : PolygonBorder
{
    public float MoistureFlow { get; private set; }
    public GeoPolygonBorder(Polygon poly1, Polygon poly2, List<LineSegment> segments) 
        : base(poly1, poly2, segments)
    {
    }

    public GeoPolygonBorder(Polygon poly1, List<LineSegment> poly1SegsRel, Polygon poly2, List<LineSegment> poly2SegsRel)
        : base(poly1, poly1SegsRel, poly2, poly2SegsRel)
    {
    }

    public void SetFlow(float width)
    {
        MoistureFlow = width;
    }
    public void IncrementFlow(float increment)
    {
        MoistureFlow += increment;
    }
}