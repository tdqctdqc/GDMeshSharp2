using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public Repository<GenPolygon> GeoPolygons { get; private set; }

    public PlanetDomain(Data data) : base()
    {
        GeoPolygons = new Repository<GenPolygon>(this, data);
        _repos.Add(typeof(GenPolygon), GeoPolygons);
    }
}