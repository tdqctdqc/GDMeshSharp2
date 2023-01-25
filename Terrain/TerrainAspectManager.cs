using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspectManager<TAspect> : IModelRepo<TAspect>
    where TAspect : TerrainAspect
{
    public Dictionary<string, TAspect> ByName { get; private set; }
    public List<TAspect> ByPriority { get; private set; }
    public TAspect LandDefault { get; protected set; } 
    public TAspect WaterDefault { get; protected set; }
    Dictionary<string, TAspect> IModelRepo<TAspect>.Models => ByName;
    public TerrainAspectManager(TAspect waterDefault, 
        TAspect landDefault, List<TAspect> byPriority)
    {
        WaterDefault = waterDefault;
        LandDefault = landDefault;
        ByPriority = byPriority;
        ByName = new Dictionary<string, TAspect>();
        ByName.Add(waterDefault.Name, waterDefault);
        if(landDefault != waterDefault) ByName.Add(landDefault.Name, landDefault);
        ByPriority.ForEach(ta => ByName.Add(ta.Name, ta));
    }

    public void BuildTriHolders(IDDispenser id, Data data, CreateWriteKey key)
    {
        add(WaterDefault);
        add(LandDefault);
        ByPriority.ForEach(add);

        void add(TerrainAspect aspect)
        {
            if (data.Planet.TerrainTris.ByName.ContainsKey(aspect.Name)) return;
            var triHolder = new TerrainTriHolder(aspect, id.GetID(), key);
            data.AddEntity(triHolder, typeof(PlanetDomain), key);
        }
    }
    public TAspect GetAspectFromPoly(MapPolygon p, WorldData data)
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

    public TAspect GetAspectAtPoint(MapPolygon p, Vector2 offsetFromPolyCenter, Data data)
    {
        var tris = data.Planet.TerrainTris;

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

    
    public Dictionary<TAspect, List<List<MapPolygon>>> GetUnions(HashSet<MapPolygon> polys, WorldData data)
    {
        var result = new Dictionary<TAspect, List<List<MapPolygon>>>();
        //todo need to allow overlaps
        for (var i = 0; i < ByPriority.Count; i++)
        {
            var aspect = ByPriority[i];
            var union = GetAspectUnion(aspect, data);
            result.Add(aspect, union.Where(u => aspect.Allowed(u[0], data)).ToList());
        }
        
        return result;
    }

    private List<List<MapPolygon>> GetAspectUnion(TAspect aspect, WorldData data)
    {
        var polys = data.Planet.Polygons.Entities.Where(p => aspect.Allowed(p, data));
        return UnionFind<MapPolygon, float>.DoUnionFind(polys.ToList(), 
            (p1, p2) => p1.HasNeighbor(p2),
            poly => poly.Neighbors.Refs()
        );
    }
    
    public void BuildTris(HashSet<MapPolygon> affectedPolys, WorldData data)
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
    public void BuildTrisForAspect(TAspect aspect, WorldData data, List<MapPolygon> affectedPolys = null)
    {
        if (aspect == LandDefault || aspect == WaterDefault) return;
        if (affectedPolys == null) affectedPolys = data.Planet.Polygons.Entities.Where(p => aspect.Allowed(p, data)).ToList();
        int triCount = 0;
        affectedPolys.ForEach(p =>
        {
            var tris = aspect.TriBuilder.BuildTrisForPoly(p, data);
            triCount += tris.Count;
            AddTris(aspect, p, tris, data);
        });
    }
    public void AddTris(TAspect aspect, MapPolygon p, List<Triangle> trisRel, Data data)
    {
        if (trisRel == null || trisRel.Count == 0) return;
        data.Planet.TerrainTris.GetTris(aspect).AddTris(p, trisRel);
    }
}