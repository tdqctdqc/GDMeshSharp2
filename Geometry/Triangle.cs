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
        if (this.IsDegenerate()) throw new Exception();
    }

    public Triangle Transpose(Vector2 offset)
    {
        return new Triangle(A + offset, B + offset, C + offset);
    }

    public void DoForEachPoint(Action<Vector2> action)
    {
        action(A);
        action(B);
        action(C);
    }
    public Vector2 GetCentroid()
    {
        return (A + B + C) / 3f;
    }

    public bool InSection(Vector2 startRot, Vector2 endRot)
    {
        return startRot.AngleTo(A) > 0f && endRot.AngleTo(A) < 0f
               || startRot.AngleTo(B) > 0f && endRot.AngleTo(B) < 0f
               || startRot.AngleTo(C) > 0f && endRot.AngleTo(C) < 0f;
    }
    public bool IntersectsRay(Vector2 ray)
    {
        return GeometryExt.LineSegmentsIntersect(Vector2.Zero, ray, A, B)
               || GeometryExt.LineSegmentsIntersect(Vector2.Zero, ray, B, C)
               || GeometryExt.LineSegmentsIntersect(Vector2.Zero, ray, C, A);
    }
    public override string ToString()
    {
        return $"({A}, {B}, {C}";
    }
}


