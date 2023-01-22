using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LineSegment
{
    public Vector2 From { get; private set; }
    public Vector2 To { get; private set; }
    public Vector2 Mid() => (From + To) / 2f;
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
    public bool PointsTo(LineSegment l)
    {
        return l.From == To;
    }

    public float DistanceTo(Vector2 pointRel)
    {
        return pointRel.DistToLine(From, To);
    }

    public float Length()
    {
        return From.DistanceTo(To);
    }
}