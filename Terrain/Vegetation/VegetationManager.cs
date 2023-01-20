using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationManager : TerrainAspectManager<Vegetation>
{
    public static Vegetation Swamp = new Swamp();
    
    public static Vegetation Forest = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .5f, Colors.DarkGreen, "Forest", new BlobTriBuilder());
    
    public static Vegetation Grassland = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .3f, Colors.Limegreen, "Grassland", new BlobTriBuilder());
    
    public static Vegetation Desert = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        0f, Colors.Tan, "Desert", new BlobTriBuilder());
    
    public static Vegetation Barren = new Vegetation(
        new HashSet<Landform>{LandformManager.Mountain, LandformManager.Peak}, 
        0f, Colors.Black, "Barren", null);


    public VegetationManager(CreateWriteKey key, IDDispenser id, Data data) 
        : base(id, key, Barren, Barren, new List<Vegetation>{Swamp, Forest, Grassland, Desert}, data)
    {
        
    }
    

}