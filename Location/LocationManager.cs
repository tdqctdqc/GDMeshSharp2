using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LocationManager
{
    public List<Settlement> Settlements { get; private set; }
    public HashSet<Edge<GeoPolygon>> Roads { get; private set; }
    public LocationManager()
    {
        Settlements = new List<Settlement>();
        Roads = new HashSet<Edge<GeoPolygon>>();
    }
}