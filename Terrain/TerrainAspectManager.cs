using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspectManager<TAspect> 
    where TAspect : TerrainAspect
{
    public List<TAspect> ByPriority { get; private set; }
    public TAspect LandDefault { get; protected set; } 
    public TAspect WaterDefault { get; protected set; } 
    // public Dictionary<TAspect, TerrainAspectHolder> Holders { get; private set; }

    public TerrainAspectManager(IDDispenser id, CreateWriteKey key, TAspect waterDefault, 
        TAspect landDefault, List<TAspect> byPriority, Data data)
    {
        WaterDefault = waterDefault;
        LandDefault = landDefault;
        ByPriority = byPriority;
        add(WaterDefault);
        add(LandDefault);
        byPriority.ForEach(add);

        void add(TerrainAspect aspect)
        {
            if (data.GetDomain<PlanetDomain>().TerrainTris.ByName.ContainsKey(aspect.Name)) return;
            var triHolder = new TerrainTriHolder(aspect, id.GetID(), key);
            data.AddEntity(triHolder, typeof(PlanetDomain), key);
        }
    }
    public TAspect GetAspectFromPoly(GenPolygon p, WorldData data)
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

    public TAspect GetAspectAtPoint(GenPolygon p, Vector2 offsetFromPolyCenter, Data data)
    {
        var tris = data.GetDomain<PlanetDomain>().TerrainTris;

        for (int i = 0; i < ByPriority.Count; i++)
        {
            if (tris.GetTris(ByPriority[i]).Contains(p, offsetFromPolyCenter)) 
                return ByPriority[i];
        }
        if (p.IsWater())
        {
            return WaterDefault;
        }
        return LandDefault;
    }

    
    public Dictionary<TAspect, List<List<GenPolygon>>> GetUnions(HashSet<GenPolygon> polys, WorldData data)
    {
        var result = new Dictionary<TAspect, List<List<GenPolygon>>>();
        //todo need to allow overlaps
        for (var i = 0; i < ByPriority.Count; i++)
        {
            var aspect = ByPriority[i];
            var union = GetAspectUnion(aspect, data);
            result.Add(aspect, union.Where(u => aspect.Allowed(u[0], data)).ToList());
        }
        
        return result;
    }

    private List<List<GenPolygon>> GetAspectUnion(TAspect aspect, WorldData data)
    {
        var polys = data.PlanetDomain.GeoPolygons.Entities.Where(p => aspect.Allowed(p, data));
        return UnionFind<GenPolygon, float>.DoUnionFind(polys.ToList(), 
            (p1, p2) => p1.HasNeighbor(p2),
            poly => poly.GeoNeighbors.Refs
        );
    }
    
    public void BuildTris(HashSet<GenPolygon> affectedPolys, WorldData data)
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
                AddTris(aspect, p, tris, data);
            });
        }
    }
    public void BuildTrisForAspect(TAspect aspect, WorldData data, List<GenPolygon> affectedPolys = null)
    {
        if (aspect == LandDefault || aspect == WaterDefault) return;
        if (affectedPolys == null) affectedPolys = data.PlanetDomain.GeoPolygons.Entities.Where(p => aspect.Allowed(p, data)).ToList();
        int triCount = 0;
        affectedPolys.ForEach(p =>
        {
            var tris = aspect.TriBuilder.BuildTrisForPoly(p, data);
            triCount += tris.Count;
            AddTris(aspect, p, tris, data);
        });
    }
    public void AddTris(TAspect aspect, GenPolygon p, List<Triangle> trisRel, Data data)
    {
        if (trisRel == null || trisRel.Count == 0) return;
        data.GetDomain<PlanetDomain>().TerrainTris.GetTris(aspect).AddTris(p, trisRel);
    }
}