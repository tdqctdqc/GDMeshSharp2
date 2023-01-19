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
    public static Landform Urban { get; private set; } = new Urban();
    public Dictionary<Edge<GenPolygon>, float> RiverSegments { get; private set; }
    


    public LandformManager()
        : base(Water, Plain, new List<Landform>{Urban, River, Peak, Mountain, Hill})
    {
    }
}