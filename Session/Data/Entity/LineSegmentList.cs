using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[Convertible] public class LineSegmentList : EntityConvertibleVar<List<float>, List<LineSegment>>
{
    public LineSegmentList(List<LineSegment> c, CreateWriteKey key) : base(c, key)
    {
    }

    public LineSegmentList(List<float> b) : base(b)
    {
    }

    public override List<LineSegment> ConvertFromBase(List<float> b)
    {
        var res = new List<LineSegment>();
        for (var i = 0; i < b.Count; i += 4)
        {
            res.Add(new LineSegment( new Vector2(b[i], b[i + 1]),  new Vector2(b[i + 2], b[i + 3])));
        }
        return res;
    }

    public override List<float> ConvertToBase(List<LineSegment> c)
    {
        var res = new List<float>();
        for (var i = 0; i < c.Count; i += 2)
        {
            res.Add(c[i].From.x);
            res.Add(c[i].From.y);
            res.Add(c[i].To.x);
            res.Add(c[i].To.y);
        }
        return res;
    }
}