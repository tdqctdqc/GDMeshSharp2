using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class TerrainTriHolder : Entity
{
    public Dictionary<int, List<Triangle>> Tris { get; private set; }
    public ModelRef<TerrainAspect> TerrainAspect { get; private set; }
    public bool Contains(MapPolygon p, Vector2 offsetFromPolyCenter)
    {
        return Tris[p.Id].Any(t => t.PointInsideTriangle(offsetFromPolyCenter));
    }

    public TerrainTriHolder(TerrainAspect aspect, int id, CreateWriteKey key) : base(id, key)
    {
        TerrainAspect = new ModelRef<TerrainAspect>(aspect.Name);
        Tris = new Dictionary<int, List<Triangle>>();
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
    private static TerrainTriHolder DeserializeConstructor(object[] args)
    {
        return new TerrainTriHolder(args);
    }
    private TerrainTriHolder(object[] args) : base(args) { }
}