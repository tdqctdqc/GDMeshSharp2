using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandSeaManager
{
    public List<HashSet<GeoPolygon>> Landmasses { get; private set; }
    public Dictionary<GeoPolygon, HashSet<GeoPolygon>> LandmassDic { get; private set; }
    public List<HashSet<GeoPolygon>> Seas { get; private set; }
    public Dictionary<GeoPolygon, HashSet<GeoPolygon>> SeaDic { get; private set; }

    public LandSeaManager()
    {
        
    }

    public void SetLandmasses(WorldData data)
    {
        Landmasses = new List<HashSet<GeoPolygon>>();
        LandmassDic = new Dictionary<GeoPolygon, HashSet<GeoPolygon>>();
        var landPolys = data.GeoPolygons.Where(p => p.IsLand());
        var seaPolys = data.GeoPolygons.Where(p => p.IsWater());
        var landmasses =
            UnionFind<GeoPolygon, float>.DoUnionFind(landPolys.ToList(), (p1, p2) => p1.HasNeighbor(p2), p1 => p1.GeoNeighbors);
        landmasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Landmasses.Add(hash);
            m.ForEach(p => LandmassDic.Add(p, hash));
        });
        
        //todo is union find only doing elements inside the input list?
        Seas = new List<HashSet<GeoPolygon>>();
        SeaDic = new Dictionary<GeoPolygon, HashSet<GeoPolygon>>();
        var SeaPolys = data.GeoPolygons.Where(p => p.IsWater());
        var seamasses =
            UnionFind<GeoPolygon, float>.DoUnionFind(seaPolys.ToList(), 
                (p1, p2) => p1.HasNeighbor(p2), p1 => p1.GeoNeighbors);
        seamasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Seas.Add(hash);
            m.ForEach(p => SeaDic.Add(p, hash));
        });
    }
}