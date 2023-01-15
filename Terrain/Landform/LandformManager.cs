using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandformManager : TerrainAspectManager<Landform>
{
    public static Landform Peak { get; private set; } = new Landform("Peak", .8f, Colors.White, 
        new PeakTriBuilder(p => p.Roughness >= Mountain.MinRoughness));
    public static Landform Mountain { get; private set; } = new Landform("Mountain", .6f, Colors.DarkSlateGray, 
        new SpikeyTriBuilder(n => n.Roughness >= Mountain.MinRoughness, n => n.Roughness));
    public static Landform Hill { get; private set; } = new Landform("Hill", .4f, Colors.Brown, new BlobTriBuilder());
    public static Landform Plain  { get; private set; } = new Landform("Plain", 0f, Colors.SaddleBrown, new NoTriBuilder());
    public static Landform Water  { get; private set; } = new Landform("Water", 0f, Colors.Blue, new NoTriBuilder());
    public static Landform River { get; private set; } = new River();
    public Dictionary<Edge<GeologyPolygon>, float> RiverSegments { get; private set; }
    protected override Func<Landform, float> _getMin { get; set; } = l => l.MinRoughness;
    


    public LandformManager()
        : base(Water, Plain, new List<Landform>{River}, new List<Landform>{Peak, Mountain, Hill})
    {
    }
}