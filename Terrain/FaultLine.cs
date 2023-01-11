using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLine 
{
    public List<List<Vector2>> Segments { get; private set; }
    public GeologyPlate LowId { get; private set; }
    public GeologyPlate HighId { get; private set; }
    public List<GeologyPolygon> PolyFootprint { get; private set; }
    public float Friction { get; private set; }
    public FaultLine(float friction, List<List<Vector2>> segments, GeologyPlate highId, GeologyPlate lowId)
    {
        Friction = friction;
        Segments = segments;
        HighId = highId;
        LowId = lowId;
        PolyFootprint = new List<GeologyPolygon>();
    }

    public float DistToFault(Vector2 point)
    {
        float dist = Mathf.Inf;
        Segments.ForEach(seg =>
        {
            for (var i = 0; i < seg.Count - 1; i++)
            {
                var sampleDist = point.DistFromLineSegmentToPoint(seg[i], seg[i + 1]);
                if (sampleDist < dist)
                {
                    dist = sampleDist;
                }
            }
        });
        return dist;
    }

    public bool PointWithinDist(Vector2 point, float dist)
    {
        return Segments.Any(seg =>
        {
            return Enumerable.Range(0, seg.Count - 2).Any(i =>
            {
                return point.DistFromLineSegmentToPoint(seg[i], seg[i + 1]) <= dist;
            });
        });
    }
}