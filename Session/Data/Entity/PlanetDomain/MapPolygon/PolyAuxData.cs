using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class PolyAuxData
{
    public Vector2 GraphicalCenter { get; private set; }
    public List<Triangle> WheelTris { get; private set; }
    public List<LineSegment> OrderedBoundarySegs { get; private set; }
    public List<PolyBorderChain> OrderedNeighborBorders { get; private set; }
    public PolyAuxData(MapPolygon p, Data data)
    {
        var nbs = p.GetPolyBorders();
        
        if (nbs.Count() > 0)
        {
            OrderedNeighborBorders = nbs.Ordered<PolyBorderChain, Vector2>().ToList();
            OrderedBoundarySegs = BuildBoundarySegments(data);
            GraphicalCenter = OrderedBoundarySegs.Average();
            SetWheelTris(p, data);
        }
    }
    private List<LineSegment> BuildBoundarySegments(Data data)
    {
        var neighborSegs = OrderedNeighborBorders.SelectMany(b => b.Segments).ToList();

        if (neighborSegs.IsCircuit() == false)
        {
            var edge = new LineSegment(neighborSegs.Last().To, neighborSegs[0].From);
            neighborSegs.Add(edge);
        }

        if (neighborSegs.IsCircuit() == false || neighborSegs.IsContinuous() == false)
        {
            GD.Print("still not circuit");
            throw new Exception();
            // throw new SegmentsNotConnectedException(before, neighborSegs);
        }
        return neighborSegs;
    }
    
    private void SetWheelTris(MapPolygon p, Data data)
    {
        WheelTris = new List<Triangle>();
        if (p.Neighbors.Entities().Count == 0)
        {
            throw new Exception();
        }
        foreach (var n in p.Neighbors.Entities())
        {
            var segs = p.GetBorder(n).Segments;
            for (var j = 0; j < segs.Count; j++)
            {
                var tri = new Triangle(Vector2.Zero, segs[j].From, segs[j].To);
                WheelTris.Add(tri);
            }
        }
    }
}
