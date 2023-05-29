using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct LineSegStruct
{
    public Vector2 A { get; private set; }
    public Vector2 B { get; private set; }

    public LineSegStruct(LineSegment ls)
    {
        A = Vector2.Zero;
        B = Vector2.Zero;
        Assign(ls.From, ls.To);
    }
    public LineSegStruct(Vector2 a, Vector2 b)
    {
        A = Vector2.Zero;
        B = Vector2.Zero;
        Assign(a, b);
    }
    private void Assign(Vector2 a, Vector2 b)
    {
        if (a.x != b.x)
        {
            if (a.x < b.x)
            {
                A = a;
                B = b;
            }
            else
            {
                B = a;
                A = b;
            }
        }
        else if (a.y != b.y)
        {
            if (a.y < b.y)
            {
                A = a;
                B = b;
            }
            else
            {
                B = a;
                A = b;
            }
        }
        else
        {
            A = a;
            B = b;
        }
    }
}
