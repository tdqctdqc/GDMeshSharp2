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
    public PolyAuxData(MapPolygon p, Data data)
    {
        var nbs = p.NeighborBorders.Values.ToList();
        if (nbs.Count() > 0)
        {
            OrderedBoundarySegs = BuildBoundarySegments(p, data);
            GraphicalCenter = OrderedBoundarySegs.Average();
            SetWheelTris(p, data);
        }
    }
    private List<LineSegment> BuildBoundarySegments(MapPolygon p, Data data)
    {
        var source = p.Neighbors.Select(n => p.GetBorder(n.Id).Segments).ToList();
        List<LineSegment> ordered;
        try
        {
            ordered = source.ToList().Chainify();
        }
        catch (Exception e)
        {
            GD.Print(p.Center);
            GD.Print(p.Neighbors.Count() + " neighbors");
            throw;
        }
        for (var i = 0; i < ordered.Count - 1; i++)
        {
            var thisSeg = ordered[i];
            var nextSeg = ordered[i + 1];
            if (thisSeg.From == nextSeg.To && thisSeg.To == nextSeg.From)
            {
                var e = new SegmentsException("retracking boundary seg");
                e.AddSegLayer(source.SelectMany(l => l).ToList(), "source");
                e.AddSegLayer(ordered, "ordered");
            
                var nList = p.Neighbors.ToList();
                for (var j = 0; j < nList.Count; j++)
                {
                    var n = nList[j];
                    var edge = p.GetEdge(n, data);
                    e.AddSegLayer(edge.GetSegsRel(p).Segments, "n " + j);
                }
                throw e;
            }
        }
        
        return ordered.ToList().Circuitify();
    }
    
    private void SetWheelTris(MapPolygon p, Data data)
    {
        WheelTris = new List<Triangle>();
        if (p.Neighbors.RefIds.Count == 0)
        {
            throw new Exception();
        }
        foreach (var n in p.Neighbors.RefIds)
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
