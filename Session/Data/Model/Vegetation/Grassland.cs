
using System.Collections.Generic;
using Godot;

public class Grassland : Vegetation
{
    public Grassland() 
        : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
            .3f, 1f, Colors.Limegreen, "Grassland", true)
    {
    }

}
