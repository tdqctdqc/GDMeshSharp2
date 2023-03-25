using System;
using System.Collections.Generic;
using System.Linq;

public class PolyTriPosition
{
    public MapPolygon Poly { get; private set; }
    public PolyTri Tri { get; private set; }

    public PolyTriPosition(MapPolygon poly, PolyTri tri)
    {
        Poly = poly;
        Tri = tri;
    }
}
