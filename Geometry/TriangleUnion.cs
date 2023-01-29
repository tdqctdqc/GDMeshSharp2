using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TriangleUnion
{
    public List<Triangle> Tris { get; private set; }
    public TriangleUnion(Triangle seed)
    {
        Tris.Add(seed);
    }

    public void AddTri(Triangle tri)
    {
        
    }
}