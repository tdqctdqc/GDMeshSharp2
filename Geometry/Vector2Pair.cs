using Godot;
using System;

public struct Vector2Pair
{
    public Vector2 V { get; set; }

    public Vector2 W { get; set; }

    public Vector2Pair(Vector2 v1, Vector2 v2)
    {
        if (v1 == v2) throw new Exception();
        Vector2 v;
        Vector2 w;
        if (v1.x != v2.x)
        {
            v = v1.x > v2.x ? v1 : v2;
            w = v1.x > v2.x ? v2 : v1;
        }
        else
        {
            v = v1.y > v2.y ? v1 : v2;
            w = v1.y > v2.y ? v2 : v1;
        }

        V = v;
        W = w;
    }
}