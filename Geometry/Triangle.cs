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
        if (TriangleExt.IsClockwise(a, b, c))
        {
            B = c;
            C = b;
        }
        // if (this.IsDegenerate()) throw new Exception("Triangle is degenerate");
    }

    public Triangle Transpose(Vector2 offset)
    {
        return new Triangle(A + offset, B + offset, C + offset);
    }

    public void ReplacePoint(Vector2 oldPoint, Vector2 newPoint)
    {
        if (oldPoint == A)
        {
            A = newPoint;
        }
        else if (oldPoint == B)
        {
            B = newPoint;
        }
        else if (oldPoint == C)
        {
            C = newPoint;
        }
        else
        {
            throw new Exception();
        }
    }
    public bool HasPoint(Vector2 v)
    {
        return v == A || v == B || v == C;
    }
    public void ForEachPoint(Action<Vector2> action)
    {
        action(A);
        action(B);
        action(C);
    }

    public bool AllPoints(Func<Vector2, bool> pred)
    {
        return pred(A) && pred(B) && pred(C);
    }
    public bool AnyPoint(Func<Vector2, bool> pred)
    {
        return pred(A) || pred(B) || pred(C);
    }
    public bool AllPointPairs(Func<Vector2, Vector2, bool> pred)
    {
        return pred(A, B) && pred(B, C) && pred(C, A);
    }
    
    public bool AnyPointPairs(Func<Vector2, Vector2, bool> pred)
    {
        return pred(A, B) || pred(B, C) || pred(C, A);
    }

    public Vector2 GetNext(Vector2 v)
    {
        if (v == A) return B;
        if (v == B) return C;
        if (v == C) return A;
        throw new Exception("point is not part of tri");
    }
    public Vector2 GetCentroid()
    {
        return (A + B + C) / 3f;
    }

    public bool InSection(Vector2 startRot, Vector2 endRot)
    {
        
        return startRot.GetClockwiseAngleTo(A) < startRot.GetClockwiseAngleTo(endRot)
               || startRot.GetClockwiseAngleTo(B) < startRot.GetClockwiseAngleTo(endRot)
               || startRot.GetClockwiseAngleTo(C) < startRot.GetClockwiseAngleTo(endRot);
        // return startRot.AngleTo(A) > 0f && endRot.AngleTo(A) < 0f
        //        || startRot.AngleTo(B) > 0f && endRot.AngleTo(B) < 0f
        //        || startRot.AngleTo(C) > 0f && endRot.AngleTo(C) < 0f;
    }
    public bool IntersectsRay(Vector2 ray)
    {
        return Vector2Ext.LineSegmentsIntersect(Vector2.Zero, ray, A, B)
               || Vector2Ext.LineSegmentsIntersect(Vector2.Zero, ray, B, C)
               || Vector2Ext.LineSegmentsIntersect(Vector2.Zero, ray, C, A);
    }
    public override string ToString()
    {
        return $"({A}, {B}, {C}";
    }

    public Vector2 GetDimensions()
    {
        return new Vector2(Mathf.Abs(MaxX() - MinX()), Mathf.Abs(MaxY() - MinY()));
    }
    public float MinX()
    {
        return Mathf.Min(A.x, Mathf.Min(B.x, C.x));
    }
    public float MaxX()
    {
        return Mathf.Max(A.x, Mathf.Max(B.x, C.x));
    }
    public float MinY()
    {
        return Mathf.Min(A.y, Mathf.Min(B.y, C.y));
    }
    public float MaxY()
    {
        return Mathf.Max(A.y, Mathf.Max(B.y, C.y));
    }
}


