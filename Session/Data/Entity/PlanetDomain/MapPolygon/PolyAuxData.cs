using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class PolyAuxData
{
    public bool Stale { get; private set; }
    public Vector2 GraphicalCenter { get; private set; }
    public IReadOnlyList<LineSegment> OrderedBoundarySegs => _orderedBoundarySegs;
    private List<LineSegment> _orderedBoundarySegs;
    public IReadOnlyList<Vector2> OrderedBoundaryPoints => _orderedBoundaryPoints;
    private Vector2[] _orderedBoundaryPoints;
    public PolyAuxData(MapPolygon p, Data data)
    {
        Update(p, data);
    }

    public void Update(MapPolygon p, Data data)
    {
        var nbs = p.NeighborBorders.Values.ToList();
        if (nbs.Count() > 0)
        {
            var source = p.Neighbors.Select(n => p.GetBorder(n.Id).Segments).ToList();
            MakeBoundarySegs(p, data, source);   
        }
    }

    private void MakeBoundarySegs(MapPolygon p, Data data, List<List<LineSegment>> source)
    {
        var ordered = source.Chainify();
        if (ordered.IsChain() == false)
        {
            var e = new SegmentsException("couldnt order boundary");
            e.AddSegLayer(_orderedBoundarySegs, "old");
            e.AddSegLayer(ordered, "new");
            throw e;
        }
        ordered.CompleteCircuit();

        _orderedBoundarySegs = ordered;
        _orderedBoundaryPoints = ordered.GetPoints().ToArray();
        GraphicalCenter = OrderedBoundarySegs.Average();
    }

    public bool PointInPoly(Vector2 pointRel)
    {
        return Geometry.IsPointInPolygon(pointRel, _orderedBoundaryPoints);
    }
    public void MarkStale(GenWriteKey key)
    {
        Stale = true;
    }

    public void MarkFresh()
    {
        Stale = false;
    }
}
