using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LineSegment
{
    public Vector2 From { get; set; }

    public Vector2 To { get; set; }
    public LineSegment(Vector2 from, Vector2 to)
    {
        From = from;
        To = to;
    }

    public LineSegment GetReverse()
    {
        return new LineSegment(To, From);
    }

    public LineSegment ChangeOrigin(Vector2 oldOrigin, Vector2 newOrigin)
    {
        return new LineSegment(From + oldOrigin - newOrigin, To + oldOrigin - newOrigin);
    }

    public bool PointsTo(LineSegment l)
    {
        return l.From == To;
    }
}