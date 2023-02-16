using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationManager : TerrainAspectManager<Vegetation>
{
    public static Vegetation Swamp = new Swamp();
    
    public static Vegetation Forest = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .5f, .25f, Colors.DarkGreen, "Forest", false, new BlobTriBuilder());
    
    public static Vegetation Grassland = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .3f, 1f, Colors.Limegreen, "Grassland", true, new BlobTriBuilder());
    
    public static Vegetation Desert = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        0f, .1f, Colors.Tan, "Desert", true, new BlobTriBuilder());
    
    public static Vegetation Barren = new Vegetation(
        new HashSet<Landform>{LandformManager.Mountain, LandformManager.Peak, LandformManager.Urban, LandformManager.River, LandformManager.Sea}, 
        0f, 0f, Colors.Transparent, "Barren", true, null);


    public VegetationManager() 
        : base(Barren, Barren, new List<Vegetation>{Swamp, Forest, Grassland, Desert, Barren})
    {
        
    }
    public Vegetation GetAtPoint(MapPolygon poly, Vector2 pRel, Landform lf, Data data)
    {
        var close = poly.Neighbors.Refs().OrderBy(n => (poly.GetOffsetTo(n, data) - pRel).Length());
        var first = close.ElementAt(0);
        var second = close.ElementAt(1);
        var score = poly.GetScore(first, second, pRel, data, p => p.Moisture);
        try
        {
            return ByPriority.First(v => v.Allowed(poly, score, lf, data));
        }
        catch (Exception e)
        {
            GD.Print($"cant find veg for lf {lf.Name} and moisture score {score}");
            throw;
        }
        
    }

}