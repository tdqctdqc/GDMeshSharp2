
using System;
using Godot;

public class PolyTri : Triangle
{
    public int Id { get; private set; }
    public Landform Landform { get; set; }
    public Vegetation Vegetation { get; set; }
    public int NumNeighborsInPoly { get; private set; }
    public int NumNeighborsOutsidePoly { get; private set; }

    public PolyTri(int id, Vector2 a, Vector2 b, Vector2 c, Landform landform, Vegetation vegetation)
    : base(a,b,c)
    {
        Landform = landform;
        Vegetation = vegetation;
    }
    public PolyTri(Vector2 a, Vector2 b, Vector2 c)
        : base(a,b,c)
    {
    }

    public void DoForEachNeighbor(Action<PolyTri, PolyTri> action)
    {
        
    }
}
