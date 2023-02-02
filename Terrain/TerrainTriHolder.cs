using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TerrainTriHolder : Entity
{
    public override Type GetDomainType() => typeof(PlanetDomain);
    public TerrainTriDic Tris { get; private set; }
    public ModelRef<TerrainAspect> TerrainAspect { get; private set; }
    public bool Contains(MapPolygon p, Vector2 offsetFromPolyCenter)
    {
        if (Tris.ContainsKey(p.Id) == false) return false;
        return Tris[p.Id].Any(t => t.PointInsideTriangle(offsetFromPolyCenter));
    }
    public Triangle TriangleContaining(MapPolygon p, Vector2 offsetFromPolyCenter)
    {
        if (Tris.ContainsKey(p.Id) == false) return null;
        return Tris[p.Id].FirstOrDefault(t => t.PointInsideTriangle(offsetFromPolyCenter));
    }

    [SerializationConstructor] private TerrainTriHolder(int id, TerrainTriDic tris, 
        ModelRef<TerrainAspect> terrainAspect) : base(id)
    {
        Tris = tris;
        TerrainAspect = terrainAspect;
    }

    public static TerrainTriHolder Create(TerrainAspect aspect, int id, CreateWriteKey key)
    {
        var terrainAspect = new ModelRef<TerrainAspect>(aspect.Name);
        var tris = new TerrainTriDic();
        var th =  new TerrainTriHolder(id, tris, terrainAspect);
        key.Create(th);
        return th;
    }
    public void AddTri(MapPolygon p, Triangle tri)
    {
        //todo make procedure
        if(Tris.ContainsKey(p.Id) == false) Tris.Add(p.Id, new List<Triangle>());
        Tris[p.Id].Add(tri);
    }
    public void AddTris(MapPolygon p, List<Triangle> tris)
    {
        //todo make procedure
        if(Tris.ContainsKey(p.Id) == false) Tris.Add(p.Id, new List<Triangle>());
        Tris[p.Id].AddRange(tris);
    }

    public List<Triangle> GetPolyTris(MapPolygon p)
    {
        if (Tris.ContainsKey(p.Id)) return Tris[p.Id];
        return null;
    }
}