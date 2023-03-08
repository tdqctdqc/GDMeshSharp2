
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Clockwise
{
    public static void OrderByClockwise<T>(this List<T> elements, 
        Vector2 center, 
        Func<T, Vector2> elPos)
    {
        var first = elPos(elements.First()) - center;
        
        elements.Sort((i,j) => 
            (elPos(i) - center).GetClockwiseAngleTo(first)
            .CompareTo(
                (elPos(j) - center).GetClockwiseAngleTo(first)
            )
        );
    }
    
    public static bool IsClockwise(this LineSegment seg, Vector2 center)
    {
        return IsCCW(seg.From, seg.To, center) == false;
    }
    public static bool IsCCW(this LineSegment seg, Vector2 center)
    {
        return IsCCW(seg.From, seg.To, center);
    }
    public static bool IsClockwise(Vector2 a, Vector2 b, Vector2 c)
    {
        return IsCCW(a, b, c) == false;
    }
    public static bool IsCCW(Vector2 a, Vector2 b, Vector2 c)
    {
        var cross = (a - b).Cross(c - b);
        return (a - b).Cross(c - b) < 0f;
    }
    public static void CorrectSegmentsToClockwise(this List<LineSegment> segs, Vector2 center)
    {
        if (segs.IsConvexAround(center) == false) throw new Exception();
        for (var i = 0; i < segs.Count; i++)
        {
            if (IsClockwise(segs[i], center) == false) segs[i] = segs[i].Reverse();
        }
    }
    public static void CorrectSegmentsToCCW(this List<LineSegment> segs, Vector2 center)
    {
        if (segs.IsConvexAround(center) == false) throw new Exception();
        for (var i = 0; i < segs.Count; i++)
        {
            if (IsClockwise(segs[i], center)) segs[i] = segs[i].Reverse();
        }
    }
}
