
using System;
using Godot;

public class ArctanMetric : IMetric<float>
{
    private float _squarenessFactor;
    private static float _tanHalf = Mathf.Tan(.5f);

    public ArctanMetric(float halfwayX)
    {
        _squarenessFactor = _tanHalf / halfwayX;
    }

    public float GetMetric(float t)
    {
        return Mathf.Atan(t * _squarenessFactor) / (Mathf.Pi / 2f);
    }
}
