using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspectManager<TAspect> where TAspect: TerrainAspect
{
    public List<TAspect> LandByPriority { get; private set; }
    public List<TAspect> WaterByPriority { get; private set; }
    public TAspect LandDefault { get; protected set; } 
    public TAspect WaterDefault { get; protected set; } 
    public Dictionary<TAspect, TerrainAspectHolder> Holders { get; private set; }
    protected abstract  Func<TAspect, float> _getMin { get; set; }

    public TerrainAspectManager(TAspect waterDefault, TAspect landDefault, List<TAspect> waterValues, 
        List<TAspect> landValues)
    {
        WaterDefault = waterDefault;
        LandDefault = landDefault;
        
        LandByPriority = landValues.OrderByDescending(_getMin).ToList();
        WaterByPriority = waterValues.OrderByDescending(_getMin).ToList();
        Holders = new Dictionary<TAspect, TerrainAspectHolder>();
        LandByPriority.ForEach(a => Holders.Add(a, new TerrainAspectHolder()));
        WaterByPriority.ForEach(a => Holders.Add(a, new TerrainAspectHolder()));
    }
    public TAspect GetValueFromPoly(GeologyPolygon p)
    {
        if (p.IsWater)
        {
            for (var i = 0; i < WaterByPriority.Count; i++)
            {
                var val = WaterByPriority[i];
                if (val.Allowed(p))
                {
                    return val;
                }
            }
            return WaterDefault;
        }
        else
        {
            for (var i = 0; i < LandByPriority.Count; i++)
            {
                var val = LandByPriority[i];
                if (val.Allowed(p))
                {
                    return val;
                }
            }
            return LandDefault;
        }
    }

    public TAspect GetLandformAtPoint(GeologyPolygon p, Vector2 offsetFromPolyCenter)
    {
        if (p.IsLand == false)
        {
            for (int i = 0; i < WaterByPriority.Count; i++)
            {
                if (Holders[WaterByPriority[i]].Contains(p, offsetFromPolyCenter)) 
                    return WaterByPriority[i];
            }
            return WaterDefault;
        }
        for (var i = 0; i < LandByPriority.Count; i++)
        {
            if (Holders[LandByPriority[i]].Contains(p, offsetFromPolyCenter)) 
                return LandByPriority[i];
        }
        return LandDefault;
    }

    
    public Dictionary<TAspect, List<List<GeologyPolygon>>> GetUnions(HashSet<GeologyPolygon> polys)
    {
        var result = new Dictionary<TAspect, List<List<GeologyPolygon>>>();
        //todo need to allow overlaps
        var aspects = LandByPriority.Union(WaterByPriority).ToList();
        for (var i = 0; i < aspects.Count; i++)
        {
            var aspect = aspects[i];
            var union = UnionFind<GeologyPolygon, float>.DoUnionFind(polys.ToList(), 
                (p1, p2) => compare(aspect, p1, p2),
                poly => poly.GeoNeighbors
            );
            result.Add(aspect, union.Where(u => aspect.Allowed(u[0])).ToList());
        }
        bool compare(TAspect aspect, GeologyPolygon p1, GeologyPolygon p2)
        {
            return aspect.Allowed(p1) == aspect.Allowed(p2);
        }
        return result;
    }
    
    public void BuildTris(HashSet<GeologyPolygon> affectedPolys)
    {
        var aspectUnions = GetUnions(affectedPolys);
        foreach (var keyValuePair in aspectUnions)
        {
            var aspect = keyValuePair.Key;
            if (aspect == LandDefault || aspect == WaterDefault) continue; 

            var unions = keyValuePair.Value;
            unions.SelectMany(u => u).ToList().ForEach(p =>
            {
                var tris = aspect.TriBuilder.BuildTrisForPoly(p);
                AddTris(aspect, p, tris);
            });
        }
    }
    public void AddTris(TAspect aspect, GeologyPolygon p, List<Triangle> trisRel)
    {
        if (trisRel == null || trisRel.Count == 0) return;
        Holders[aspect].AddTris(p, trisRel);
    }
}