using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class PolyEdgeAux : EntityAux<MapPolygonEdge>
{
    private Entity1to1PropIndexer<MapPolygonEdge, Vector2> _byEdge;
    public PolyEdgeAux(Domain domain, Data data) : base(domain, data)
    {
        _byEdge = Entity1to1PropIndexer<MapPolygonEdge, Vector2>.CreateConstant(data, MakeEdge);
    }
    private Vector2 MakeEdge(MapPolygonEdge e)
    {
        return new Vector2(e.HighId.RefId, e.LowId.RefId);
    }
    private Vector2 MakeEdge(int p1, int p2)
    {
        var hi = p1 > p2 ? p1 : p2;
        var lo = p1 > p2 ? p2 : p1;
        
        return new Vector2(hi, lo);
    }
    public MapPolygonEdge GetEdge(MapPolygon p1, MapPolygon p2)
    {
        if (p1.HasNeighbor(p2) == false) throw new Exception();
        if (p2.HasNeighbor(p1) == false) throw new Exception();
        var e = MakeEdge(p1.Id, p2.Id);
        return _byEdge[e];
    }
}