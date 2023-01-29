using Godot;
using System;

public class Triangle 
{
    public Vector2 A { get; private set; }
    public Vector2 B { get; private set; }
    public Vector2 C { get; private set; }

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
    }

    public Triangle Transpose(Vector2 offset)
    {
        return new Triangle(A + offset, B + offset, C + offset);
    }
}


