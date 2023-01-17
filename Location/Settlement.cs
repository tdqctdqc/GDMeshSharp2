using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Settlement : Location
{
    public GeoPolygon Poly { get; private set; }
    public float Size { get; private set; }

    public Settlement(GeoPolygon poly, float size)
    {
        Poly = poly;
        Size = size;
    }
}