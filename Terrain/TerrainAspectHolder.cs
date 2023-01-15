using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainAspectHolder
{
    public Dictionary<Polygon, List<Triangle>> Tris { get; private set; }
    
    public bool Contains(Polygon p, Vector2 offsetFromPolyCenter)
    {
        return Tris[p].Any(t => t.PointInsideTriangle(offsetFromPolyCenter));
    }

    public TerrainAspectHolder()
    {
        Tris = new Dictionary<Polygon, List<Triangle>>();
    }

    public void AddTri(Polygon p, Triangle tri)
    {
        if(Tris.ContainsKey(p) == false) Tris.Add(p, new List<Triangle>());
        Tris[p].Add(tri);
    }
    public void AddTris(Polygon p, List<Triangle> tris)
    {
        if(Tris.ContainsKey(p) == false) Tris.Add(p, new List<Triangle>());
        Tris[p].AddRange(tris);
    }
}