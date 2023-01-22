using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandSeaManager
{
    public List<HashSet<MapPolygon>> Landmasses { get; private set; }
    public Dictionary<MapPolygon, HashSet<MapPolygon>> LandmassDic { get; private set; }
    public List<HashSet<MapPolygon>> Seas { get; private set; }
    public Dictionary<MapPolygon, HashSet<MapPolygon>> SeaDic { get; private set; }

    public LandSeaManager()
    {
        
    }

    public void SetLandmasses(Data data)
    {
        Landmasses = new List<HashSet<MapPolygon>>();
        LandmassDic = new Dictionary<MapPolygon, HashSet<MapPolygon>>();
        var landPolys = data.Planet.Polygons.Entities.Where(p => p.IsLand());
        var seaPolys = data.Planet.Polygons.Entities.Where(p => p.IsWater());
        var landmasses =
            UnionFind<MapPolygon, float>.DoUnionFind(landPolys.ToList(), (p1, p2) => p1.HasNeighbor(p2), p1 => p1.Neighbors.Refs());
        landmasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Landmasses.Add(hash);
            m.ForEach(p => LandmassDic.Add(p, hash));
        });
        
        //todo is union find only doing elements inside the input list?
        Seas = new List<HashSet<MapPolygon>>();
        SeaDic = new Dictionary<MapPolygon, HashSet<MapPolygon>>();
        var SeaPolys = data.Planet.Polygons.Entities.Where(p => p.IsWater());
        var seamasses =
            UnionFind<MapPolygon, float>.DoUnionFind(seaPolys.ToList(), 
                (p1, p2) => p1.HasNeighbor(p2), p1 => p1.Neighbors.Refs());
        seamasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Seas.Add(hash);
            m.ForEach(p => SeaDic.Add(p, hash));
        });
    }
}