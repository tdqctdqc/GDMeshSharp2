using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonRepository : Repository<MapPolygon>
{
    
    public MapPolygonRepository(Domain domain, Data data) : base(domain, data)
    {
    }

    public List<List<MapPolygon>> GetPlateUnions()
    {
        return UnionFind<MapPolygon, int>.DoUnionFind(
            Entities,
            (p1, p2) => p1.PlateId == p2.PlateId,
            p => p.Neighbors.Refs());
    }
}