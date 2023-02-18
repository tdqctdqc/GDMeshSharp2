
using System;
using Godot;

public struct V2Edge
{
    public Vector2 T1 { get; private set; }
    public Vector2 T2 { get; private set; }

    public V2Edge(Vector2 t1, Vector2 t2)
    {
        if (t1.Equals(t2)) throw new Exception();
        T1 = Larger(t1, t2) ? t1 : t2;
        T2 = T1.Equals(t1) ? t2 : t1;
    }

    private static bool Larger(Vector2 i1, Vector2 i2)
    {
        if (i1.x != i2.x) return i1.x > i2.x;
        if (i1.y != i2.y) return i1.y > i2.y;
        throw new Exception();
    }
}
