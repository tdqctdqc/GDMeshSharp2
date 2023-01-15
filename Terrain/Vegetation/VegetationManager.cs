using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationManager : TerrainAspectManager<Vegetation>
{
    public static Vegetation Swamp = new Vegetation(
        new HashSet<Landform>{LandformManager.Plain}, 
        .7f, Colors.DarkKhaki, "Swamp", new BlobTriBuilder());
    
    public static Vegetation Forest = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .5f, Colors.DarkGreen, "Forest", new BlobTriBuilder());
    
    public static Vegetation Grassland = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .2f, Colors.Limegreen, "Grassland", new BlobTriBuilder());
    
    public static Vegetation Desert = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        0f, Colors.Limegreen, "Desert", new BlobTriBuilder());
    
    public static Vegetation Barren = new Vegetation(
        new HashSet<Landform>{LandformManager.Mountain, LandformManager.Peak}, 
        0f, Colors.Limegreen, "Barren", null);


    public VegetationManager() 
        : base(Barren, Barren, new List<Vegetation>(), new List<Vegetation>{Swamp, Forest, Grassland})
    {
        
    }
    protected override Func<Vegetation, float> _getMin { get; set; } = v => v.MinMoisture;
    

}