using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class TerrainTriHolder : Entity
{
    public Dictionary<int, List<Triangle>> Tris { get; private set; }
    public string TerrainAspectName { get; private set; }
    public bool Contains(MapPolygon p, Vector2 offsetFromPolyCenter)
    {
        return Tris[p.Id].Any(t => t.PointInsideTriangle(offsetFromPolyCenter));
    }

    public TerrainTriHolder(TerrainAspect aspect, int id, CreateWriteKey key) : base(id, key)
    {
        TerrainAspectName = aspect.Name;
        Tris = new Dictionary<int, List<Triangle>>();
    }
    public TerrainTriHolder(TerrainAspect aspect, Dictionary<int, List<Triangle>> tris, int id, CreateWriteKey key) : base(id, key)
    {
        TerrainAspectName = aspect.Name;
        Tris = tris;
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
    private static TerrainTriHolder DeserializeConstructor(string json)
    {
        return new TerrainTriHolder(json);
    }
    private TerrainTriHolder(string json) : base(json) { }
}