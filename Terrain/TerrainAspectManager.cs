using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspectManager<TAspect> where TAspect: TerrainAspect
{
    public List<TAspect> ByPriority { get; private set; } 
    public TAspect LandDefault { get; protected set; } 
    public TAspect WaterDefault { get; protected set; } 
    public Dictionary<TAspect, TerrainAspectHolder> Holders { get; private set; }

    public TerrainAspectManager(TAspect waterDefault, TAspect landDefault, List<TAspect> byPriority)
    {
        WaterDefault = waterDefault;
        LandDefault = landDefault;
        
        ByPriority = byPriority;
        Holders = new Dictionary<TAspect, TerrainAspectHolder>();
        ByPriority.ForEach(a => Holders.Add(a, new TerrainAspectHolder()));
    }
    public TAspect GetAspectFromPoly(GeoPolygon p, WorldData data)
    {
        for (var i = 0; i < ByPriority.Count; i++)
        {
            var val = ByPriority[i];
            if (val.Allowed(p, data))
            {
                return val;
            }
        }
        if (p.IsWater())
        {
            return WaterDefault;
        }
        return LandDefault;
    }

    public TAspect GetAspectAtPoint(GeoPolygon p, Vector2 offsetFromPolyCenter)
    {
        for (int i = 0; i < ByPriority.Count; i++)
        {
            if (Holders[ByPriority[i]].Contains(p, offsetFromPolyCenter)) 
                return ByPriority[i];
        }
        if (p.IsWater())
        {
            return WaterDefault;
        }
        return LandDefault;
    }

    
    public Dictionary<TAspect, List<List<GeoPolygon>>> GetUnions(HashSet<GeoPolygon> polys, WorldData data)
    {
        var result = new Dictionary<TAspect, List<List<GeoPolygon>>>();
        //todo need to allow overlaps
        for (var i = 0; i < ByPriority.Count; i++)
        {
            var aspect = ByPriority[i];
            var union = GetAspectUnion(aspect, data);
            result.Add(aspect, union.Where(u => aspect.Allowed(u[0], data)).ToList());
        }
        
        return result;
    }

    private List<List<GeoPolygon>> GetAspectUnion(TAspect aspect, WorldData data)
    {
        var polys = data.GeoPolygons.Where(p => aspect.Allowed(p, data));
        return UnionFind<GeoPolygon, float>.DoUnionFind(polys.ToList(), 
            (p1, p2) => p1.HasNeighbor(p2),
            poly => poly.GeoNeighbors
        );
    }
    
    public void BuildTris(HashSet<GeoPolygon> affectedPolys, WorldData data)
    {
        var aspectUnions = GetUnions(affectedPolys, data);
        foreach (var keyValuePair in aspectUnions)
        {
            var aspect = keyValuePair.Key;
            if (aspect == LandDefault || aspect == WaterDefault) continue; 

            var unions = keyValuePair.Value;
            unions.SelectMany(u => u).ToList().ForEach(p =>
            {
                var tris = aspect.TriBuilder.BuildTrisForPoly(p, data);
                AddTris(aspect, p, tris);
            });
        }
    }
    public void BuildTrisForAspect(TAspect aspect, WorldData data, List<GeoPolygon> affectedPolys = null)
    {
        if (aspect == LandDefault || aspect == WaterDefault) return;
        if (affectedPolys == null) affectedPolys = data.GeoPolygons.Where(p => aspect.Allowed(p, data)).ToList();
        int triCount = 0;
        affectedPolys.ForEach(p =>
        {
            var tris = aspect.TriBuilder.BuildTrisForPoly(p, data);
            triCount += tris.Count;
            AddTris(aspect, p, tris);
        });
    }
    public void AddTris(TAspect aspect, GeoPolygon p, List<Triangle> trisRel)
    {
        if (trisRel == null || trisRel.Count == 0) return;
        Holders[aspect].AddTris(p, trisRel);
    }
}