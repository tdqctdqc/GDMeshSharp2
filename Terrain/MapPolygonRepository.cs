using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonRepository : Repository<MapPolygon>
{
    
    public MapPolygonRepository(Domain domain, Data data) : base(domain, data)
    {
    }
}