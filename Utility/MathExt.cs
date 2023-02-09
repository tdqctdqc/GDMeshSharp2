using System;
using System.Collections.Generic;
using Godot;

public static class MathExt
{
    public static float ProjectToRange(this float val, float range, float resultFloor, float cutoff)
    {
        return (val - cutoff) * (range - resultFloor) + resultFloor;
    }
}
