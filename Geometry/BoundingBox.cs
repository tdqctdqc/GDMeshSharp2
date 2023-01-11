using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BoundingBox 
{
    public float MinX { get; private set; }
    public float MaxX { get; private set; }
    public float MinY { get; private set; }
    public float MaxY { get; private set; }
    public Vector2[] CornerPoints { get; private set; }

    public BoundingBox()
    {
        MinX = Mathf.Inf;
        MaxX = -Mathf.Inf;
        MinY = Mathf.Inf;
        MaxY = -Mathf.Inf;
        CornerPoints = new Vector2[] {Vector2.Inf, Vector2.Inf, Vector2.Inf, Vector2.Inf};
    }

    public void RegisterPoint(Vector2 p)
    {
        MinX = Mathf.Min(MinX, p.x);
        MaxX = Mathf.Max(MaxX, p.x);
        MinY = Mathf.Min(MinY, p.y);
        MaxY = Mathf.Max(MaxY, p.y);
        CalculateCornerPoints();
    }

    public void Cover(BoundingBox b)
    {        
        MinX = Mathf.Min(MinX, b.MinX);
        MaxX = Mathf.Max(MaxX, b.MaxX);
        MinY = Mathf.Min(MinY, b.MinY);
        MaxY = Mathf.Max(MaxY, b.MaxY);
        CalculateCornerPoints();
    }

    private void CalculateCornerPoints()
    {
        CornerPoints[0] = new Vector2(MinX, MinY); //tl
        CornerPoints[1] = new Vector2(MaxX, MinY); //tr
        CornerPoints[2] = new Vector2(MaxX, MaxY); //br
        CornerPoints[3] = new Vector2(MinX, MaxY); //bl
    }
    public bool Contains(Vector2 p)
    {
        return p.x >= MinX
               && p.x <= MaxX
               && p.y >= MinY
               && p.y <= MaxY;
    }
}