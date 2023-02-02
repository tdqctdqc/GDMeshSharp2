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
        new HashSet<Landform>{LandformManager.Mountain, LandformManager.Peak}, 
        0f, 0f, Colors.Black, "Barren", true, null);


    public VegetationManager() 
        : base(Barren, Barren, new List<Vegetation>{Swamp, Forest, Grassland, Desert})
    {
        
    }
    

}