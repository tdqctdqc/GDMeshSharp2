
using System;
using Godot;

public class PolyTri
{
    public byte A, B, C;
    public void Set(int iter, byte vectorIndex)
    {
        if (iter == 0) A = vectorIndex;
        else if (iter == 1) B = vectorIndex;
        else if (iter == 2) C = vectorIndex;
        else throw new Exception();
    }

    public bool ContainsPoint(Vector2 p, Vector2[] vertices)
    {
        var t1 = vertices[A];
        var t2 = vertices[B];
        var t3 = vertices[C];
        var d1 = (p.x - t2.x) * (t1.y - t2.y) - (t1.x - t2.x) * (p.y - t2.y);
        var d2 = (p.x - t3.x) * (t2.y - t3.y) - (t2.x - t3.x) * (p.y - t3.y);
        var d3 = (p.x - t1.x) * (t3.y - t1.y) - (t3.x - t1.x) * (p.y - t1.y);

        return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
    }

    public Triangle GetTriangle(Vector2[] vertices)
    {
        return new Triangle(vertices[A], vertices[B], vertices[C]);
    }
}
