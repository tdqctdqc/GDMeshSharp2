using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandformManager
{
    public static Landform Mountain { get; private set; } = new Landform("Mountain", .75f, Colors.Gray);
    public static Landform Hill { get; private set; } = new Landform("Hill", .5f, Colors.Brown);
    public static Landform Plain  { get; private set; } = new Landform("Plain", 0f, Colors.Tan);
    public static Landform Default => Plain;
    public static Landform GetLandform(GeologyPolygon p)
    {
        if (p.Roughness >= Mountain.MinRoughness) return Mountain;
        if (p.Roughness >= Hill.MinRoughness) return Hill;
        return Default;
    }
}