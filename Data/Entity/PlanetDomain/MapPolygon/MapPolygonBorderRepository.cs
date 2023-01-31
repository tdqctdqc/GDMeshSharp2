using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonBorderRepository : Repository<MapPolygonBorder>
{
    public Dictionary<Edge<MapPolygon>, MapPolygonBorder> BordersByEdge { get; private set; }
    public MapPolygonBorderRepository(Domain domain, Data data) : base(domain, data)
    {
        BordersByEdge = new Dictionary<Edge<MapPolygon>, MapPolygonBorder>();
        data.Notices.RegisterEntityAddedCallback<MapPolygonBorder>(
            border =>
            {
                var edge = MakeEdge(border.HighId.Ref(), border.LowId.Ref());
                BordersByEdge[edge] = border;
            }
        );
        
    }

    private Edge<MapPolygon> MakeEdge(MapPolygon mp1, MapPolygon mp2)
    {
        return new Edge<MapPolygon>(mp1, mp2, (p1, p2) => p1.Id > p2.Id);
    }
    public MapPolygonBorder GetBorder(MapPolygon p1, MapPolygon p2)
    {
        return BordersByEdge[MakeEdge(p1, p2)];
    }
}