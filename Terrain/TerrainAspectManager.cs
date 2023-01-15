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
    protected abstract Func<GeologyPolygon, float> _polyMetric { get; set; }
    protected abstract  Func<TAspect, float> _getMin { get; set; }
    protected abstract  Func<GeologyPolygon, TAspect, bool> _allowed { get; set; }
    protected abstract Dictionary<GeologyPolygon, List<(Triangle, TAspect)>> _polyTris { get; set; }

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
        var metric = _polyMetric(p);
        if (p.IsWater)
        {
            for (var i = 0; i < WaterByPriority.Count; i++)
            {
                var val = WaterByPriority[i];
                if (_allowed(p, val) && metric >= _getMin(val))
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
                if (_allowed(p, val) && metric >= _getMin(val))
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

    public void AddLandformTris(TAspect aspect, GeologyPolygon p, List<Triangle> trisRel)
    {
        Holders[aspect].AddTris(p, trisRel);
    }
    
    public Dictionary<TAspect, List<List<GeologyPolygon>>> GetUnions(List<GeologyPolygon> polys)
    {
        bool compare(GeologyPolygon p1, GeologyPolygon p2)
        {
            return GetValueFromPoly(p1).Equals(GetValueFromPoly(p1));
        }
        var unions = UnionFind<GeologyPolygon, float>.DoUnionFind(polys.ToList(), 
            compare,
            poly => poly.GeoNeighbors
        );
        
        var result = new Dictionary<TAspect, List<List<GeologyPolygon>>>();
        
        for (var i = 0; i < unions.Count; i++)
        {
            var aspect = GetValueFromPoly(unions[0][0]);
            if(result.ContainsKey(aspect) == false) result.Add(aspect, new List<List<GeologyPolygon>>());
            result[aspect].Add(unions[i]);
        }

        return result;
    }
    
    public void BuildTris(List<GeologyPolygon> affectedPolys)
    {
        var unions = GetUnions(affectedPolys);
        var hash = unions.ToDictionary(pair => pair.Key, pair => pair.Value.SelectMany(p => p).ToHashSet());
        var polyLists = hash.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());
        var polyTrisMany = new Dictionary<TAspect, Dictionary<GeologyPolygon, (TAspect, List<Triangle>)>>();
        //todo foreach to have build mesh
        BuildAspects(affectedPolys);
        _polyTris = new Dictionary<GeologyPolygon, List<(Triangle, TAspect)>>();
        void BuildAspects(List<GeologyPolygon> polys)
        {
            var polyTris = new Dictionary<GeologyPolygon, List<Triangle>>();
            polys.ForEach(p =>
            {
                var aspect = GetValueFromPoly(p);
                var tris = aspect.BuildTrisForPoly(p, hash[aspect]);
                var list = new List<(Triangle, TAspect)>();
                tris.ForEach(t => list.Add(new ValueTuple<Triangle, TAspect>(t, aspect)));
                _polyTris.Add(p, list);
            });
        }
    }
}