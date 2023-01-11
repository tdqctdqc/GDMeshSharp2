using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PolygonBorder 
{
    public List<Vector2> LowPointsRel { get; private set; }
    public List<Vector2> HighPointsRel { get; private set; }
    public Polygon LowId { get; private set; }
    public Polygon HighId { get; private set; }
    
    //todo make store points clockwise for each, can just use order by clockwise 
    public PolygonBorder(Polygon poly1, Polygon poly2, List<Vector2> points)
    {
        if (poly1.Id < poly2.Id)
        {
            LowId = poly1;
            HighId = poly2;
        }
        else
        {
            LowId = poly2;
            HighId = poly1;
        }

        var segments = points.Select(p => p - HighId.Center).GetSegments();
        HighPointsRel = segments
            .OrderByClockwise(Vector2.Zero, s => s.V, segments.First())
            .GetPoints()
            .ToList();
        LowPointsRel = HighPointsRel.Select(p => p + HighId.Center - LowId.Center).Reverse().ToList();
    }
    public PolygonBorder(Polygon poly1, List<Vector2> poly1PointsRel, 
        Polygon poly2, List<Vector2> poly2PointsRel)
    {
        if (poly1.Id < poly2.Id)
        {
            LowId = poly1;
            HighId = poly2;
            LowPointsRel = poly1PointsRel.OrderByClockwise(Vector2.Zero, v => v, poly1PointsRel[0]).ToList();
            HighPointsRel = poly2PointsRel.OrderByClockwise(Vector2.Zero, v => v, poly2PointsRel[0]).ToList();
        }
        else
        {
            LowId = poly2;
            HighId = poly1;
            LowPointsRel = poly2PointsRel.OrderByClockwise(Vector2.Zero, v => v, poly2PointsRel[0]).ToList();
            HighPointsRel = poly1PointsRel.OrderByClockwise(Vector2.Zero, v => v, poly1PointsRel[0]).ToList();
        }
    }

    public List<Vector2> GetPointsRel(Polygon p)
    {
        if (p == LowId) return LowPointsRel;
        if (p == HighId) return HighPointsRel;
        throw new Exception();
    }

    public Vector2 GetOffsetToOtherPoly(Polygon p)
    {
        var other = GetOtherPoly(p);
        return GetPointsRel(p)[0] - GetPointsRel(other)[LowPointsRel.Count - 1];
    }
    public Polygon GetOtherPoly(Polygon p)
    {
        if (p == LowId) return HighId;
        if (p == HighId) return LowId;
        throw new Exception();
    }
    public List<Vector2> GetPointsAbs()
    {
        return HighPointsRel.Select(p => p + HighId.Center).ToList();
    }
    public void ReplacePoints(List<Vector2> newPointsAbsolute)
    {
        var segments = newPointsAbsolute.Select(p => p - HighId.Center).GetSegments();
        HighPointsRel = segments
            .OrderByClockwise(Vector2.Zero, s => s.V, segments.First())
            .GetPoints()
            .ToList();
        LowPointsRel = HighPointsRel.Select(p => p + HighId.Center - LowId.Center).Reverse().ToList();
    }
}
