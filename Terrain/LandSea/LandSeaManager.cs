using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandSeaManager
{
    public List<HashSet<GenPolygon>> Landmasses { get; private set; }
    public Dictionary<GenPolygon, HashSet<GenPolygon>> LandmassDic { get; private set; }
    public List<HashSet<GenPolygon>> Seas { get; private set; }
    public Dictionary<GenPolygon, HashSet<GenPolygon>> SeaDic { get; private set; }

    public LandSeaManager()
    {
        
    }

    public void SetLandmasses(WorldData data)
    {
        Landmasses = new List<HashSet<GenPolygon>>();
        LandmassDic = new Dictionary<GenPolygon, HashSet<GenPolygon>>();
        var landPolys = data.PlanetDomain.GeoPolygons.Entities.Where(p => p.IsLand());
        var seaPolys = data.PlanetDomain.GeoPolygons.Entities.Where(p => p.IsWater());
        var landmasses =
            UnionFind<GenPolygon, float>.DoUnionFind(landPolys.ToList(), (p1, p2) => p1.HasNeighbor(p2), p1 => p1.GeoNeighbors.Refs);
        landmasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Landmasses.Add(hash);
            m.ForEach(p => LandmassDic.Add(p, hash));
        });
        
        //todo is union find only doing elements inside the input list?
        Seas = new List<HashSet<GenPolygon>>();
        SeaDic = new Dictionary<GenPolygon, HashSet<GenPolygon>>();
        var SeaPolys = data.PlanetDomain.GeoPolygons.Entities.Where(p => p.IsWater());
        var seamasses =
            UnionFind<GenPolygon, float>.DoUnionFind(seaPolys.ToList(), 
                (p1, p2) => p1.HasNeighbor(p2), p1 => p1.GeoNeighbors.Refs);
        seamasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Seas.Add(hash);
            m.ForEach(p => SeaDic.Add(p, hash));
        });
    }
}