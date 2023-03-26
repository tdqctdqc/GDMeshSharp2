using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyEdgeAux : EntityAux<MapPolygonEdge>
{
    private Dictionary<Edge<int>, MapPolygonEdge> _bordersByEdge;
    public PolyEdgeAux(Domain domain, Data data) : base(domain, data)
    {
        _bordersByEdge = new Dictionary<Edge<int>, MapPolygonEdge>();
        EntityCreatedHandler<MapPolygonEdge>.Register(
            n =>
            {
                var border = n.Entity;
                var edge = MakeEdge(border.HighId.RefId, border.LowId.RefId);
                _bordersByEdge[edge] = border;
            }
        );
    }

    private Edge<int> MakeEdge(int mp1, int mp2)
    {
        return new Edge<int>(mp1, mp2, p1 => p1);
    }
    public MapPolygonEdge GetEdge(MapPolygon p1, MapPolygon p2)
    {
        if (p1.HasNeighbor(p2) == false) throw new Exception();
        if (p2.HasNeighbor(p1) == false) throw new Exception();
        var e = MakeEdge(p1.Id, p2.Id);
        if (_bordersByEdge.ContainsKey(e) == false) return null;
        return _bordersByEdge[e];
    }
}