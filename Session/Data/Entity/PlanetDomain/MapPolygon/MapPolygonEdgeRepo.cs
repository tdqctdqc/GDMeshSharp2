using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonEdgeRepo : EntityAux<MapPolygonEdge>
{
    public Dictionary<Edge<MapPolygon>, MapPolygonEdge> BordersByEdge { get; private set; }
    public MapPolygonEdgeRepo(Domain domain, Data data) : base(domain, data)
    {
        BordersByEdge = new Dictionary<Edge<MapPolygon>, MapPolygonEdge>();
        EntityCreatedHandler<MapPolygonEdge>.Register(
            n =>
            {
                var border = n.Entity;
                var edge = MakeEdge(border.HighId.Entity(), border.LowId.Entity());
                BordersByEdge[edge] = border;
            }
        );
        
    }

    private Edge<MapPolygon> MakeEdge(MapPolygon mp1, MapPolygon mp2)
    {
        return new Edge<MapPolygon>(mp1, mp2, (p1, p2) => p1.Id > p2.Id);
    }
    public MapPolygonEdge GetEdge(MapPolygon p1, MapPolygon p2)
    {
        if (p1.HasNeighbor(p2) == false) throw new Exception();
        if (p2.HasNeighbor(p1) == false) throw new Exception();
        var e = MakeEdge(p1, p2);
        if (BordersByEdge.ContainsKey(e) == false) return null;
        return BordersByEdge[e];
    }
}