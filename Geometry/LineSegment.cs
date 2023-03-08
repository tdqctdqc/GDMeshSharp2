using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;


public class LineSegment : ISegment<Vector2>
{
    public Vector2 From { get; set; }
    public Vector2 To { get; set; }
    ISegment<Vector2> ISegment<Vector2>.Reverse() => Reverse();

    public Vector2 Mid() => (From + To) / 2f;
    public LineSegment(Vector2 from, Vector2 to)
    {
        From = from;
        To = to;
    }
    
    public LineSegment Reverse()
    {
        return new LineSegment(To, From);
    }

    public LineSegment ChangeOrigin(Vector2 oldOrigin, Vector2 newOrigin)
    {
        return new LineSegment(From + oldOrigin - newOrigin, To + oldOrigin - newOrigin);
    }

    public LineSegment Translate(Vector2 offset)
    {
        return new LineSegment(From + offset, To + offset);
    }

    public void Clamp(float mapWidth)
    {
        if (Mid().x > mapWidth / 2f)
        {
            From += Vector2.Left * mapWidth;
            To += Vector2.Left * mapWidth;
        }

        if (Mid().x < -mapWidth / 2f)
        {
            From += Vector2.Right * mapWidth;
            To += Vector2.Right * mapWidth;
        }
    }

    public float DistanceTo(Vector2 point)
    {
        return point.DistToLine(From, To);
    }

    public float Length()
    {
        return From.DistanceTo(To);
    }

    public bool ContainsPoint(Vector2 p)
    {
        return (p - From).Normalized() == (To - p).Normalized();
    }

    public bool LeftOf(Vector2 point)
    {
        return (To.x - From.x)*(point.y - From.y) - (To.y - From.y)*(point.x - From.x) > 0;
    }

    public Vector2 GetNormalizedAxis()
    {
        return (To - From).Normalized();
    }
    public Vector2 GetNormalizedPerpendicular()
    {
        return (To - From).Perpendicular().Normalized();
    }
    public override string ToString()
    {
        return $"[from {From} to {To}] \b";
    }
}

