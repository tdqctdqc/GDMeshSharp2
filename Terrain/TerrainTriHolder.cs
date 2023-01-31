using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TerrainTriHolder : Entity
{
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

    public TerrainTriHolder(int id, TerrainTriDic tris, 
        ModelRef<TerrainAspect> terrainAspect) : base(id)
    {
        Tris = tris;
        TerrainAspect = terrainAspect;
    }

    public TerrainTriHolder(TerrainAspect aspect, int id, CreateWriteKey key) : base(id, key)
    {
        TerrainAspect = new ModelRef<TerrainAspect>(aspect.Name);
        Tris = new TerrainTriDic();
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