using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationManager : TerrainAspectManager<Vegetation>
{
    public static Vegetation Swamp = new Vegetation(
        new HashSet<Landform>{LandformManager.Plain}, 
        .7f, Colors.DarkKhaki, "Swamp");
    public static Vegetation Forest = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .5f, Colors.DarkGreen, "Forest");
    public static Vegetation Grassland = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .2f, Colors.Limegreen, "Grassland");
    public static Vegetation Desert = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        0f, Colors.Limegreen, "Desert");
    public static Vegetation Barren = new Vegetation(
        new HashSet<Landform>{LandformManager.Mountain, LandformManager.Peak}, 
        0f, Colors.Limegreen, "Barren");


    public VegetationManager() 
        : base(Barren, Barren, new List<Vegetation>(), new List<Vegetation>{Swamp, Forest, Grassland})
    {
        
    }

    protected override Func<GeologyPolygon, float> _polyMetric { get; set; } = p => p.Moisture;
    protected override Func<Vegetation, float> _getMin { get; set; } = v => v.MinMoisture;

    protected override Func<GeologyPolygon, Vegetation, bool> _allowed { get; set; } = (p, v) =>
    {
        var pLandform = Root.WorldData.Landforms.GetValueFromPoly(p);
        return v.AllowedLandforms.Contains(pLandform);
    };

    protected override Dictionary<GeologyPolygon, List<(Triangle, Vegetation)>> _polyTris { get; set; }
}