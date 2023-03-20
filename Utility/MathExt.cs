using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MathExt
{
    public static int GetNumDigits(this int i)
    {
        if (i == 0) return 0;
        return (int)Math.Floor((float)Math.Log10(i)) + 1;
    }
    public static float ProjectToRange(this float val, float range, float resultFloor, float cutoff)
    {
        return (val - cutoff) * (range - resultFloor) + resultFloor;
    }


    public static bool ConsecutiveIncreasing(int modulo, params int[] nums)
    {
        if (nums.Distinct().Count() < nums.Length) return false;
        var sort = nums.OrderBy(n => n).ToList();
        
        var min = nums.Min();
        var max = nums.Max();
        return max - min + 1 == nums.Length;
    }
}
