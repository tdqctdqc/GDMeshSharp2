
    using System.Collections.Generic;
    using Godot;

    public class Arid : Vegetation
    {
        public Arid() 
            : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
                .1f, .5f, Colors.YellowGreen.Lightened(.3f), "Arid", true)
        {
        }
    }
