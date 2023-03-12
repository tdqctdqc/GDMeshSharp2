using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonRepo : Repository<MapPolygon>
{
    public RepoEntityMultiIndexer<MapPolygon, Peep> PeepsInPoly { get; private set; }
    
    public MapPolygonRepo(Domain domain, Data data) : base(domain, data)
    {
        PeepsInPoly = new RepoEntityMultiIndexer<MapPolygon, Peep>(
            data,
            p => p.Home,
            nameof(Peep.Home)
        );
    }
}