using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandformManager : TerrainAspectManager<Landform>
{
    public static Landform Peak { get; private set; } = new Landform("Peak", .8f, Colors.White);
    public static Landform Mountain { get; private set; } = new Landform("Mountain", .6f, Colors.DarkSlateGray);
    public static Landform Hill { get; private set; } = new Landform("Hill", .4f, Colors.Brown);
    public static Landform Plain  { get; private set; } = new Landform("Plain", 0f, Colors.SaddleBrown);
    public static Landform Water  { get; private set; } = new Landform("Water", 0f, Colors.Blue);

    public LandformManager() 
        : base(Water, Plain, new List<Landform>(), new List<Landform>{Peak, Mountain, Hill})
    {
    }
    protected override Func<GeologyPolygon, float> _polyMetric { get; set; } = p => p.Roughness;
    protected override Func<Landform, float> _getMin { get; set; } = l => l.MinRoughness;
    protected override Func<GeologyPolygon, Landform, bool> _allowed { get; set; } = (p,l) => p.IsLand;
    protected override Dictionary<GeologyPolygon, List<(Triangle, Landform)>> _polyTris { get; set; }
}