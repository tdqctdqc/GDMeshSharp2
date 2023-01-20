using Godot;
using System;
using System.Collections.Generic;

public class WorldData : Data
{
    public PlanetDomain PlanetDomain { get; private set; }
    public SocietyDomain SocietyDomain { get; private set; }
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public GenAuxiliaryData GenAuxData { get; private set; }
    public Vector2 Dimensions { get; private set; }
    
    public WorldData(Vector2 dimensions) : base()
    {
        Dimensions = dimensions;
        GenAuxData = new GenAuxiliaryData();
        
        PlanetDomain = new PlanetDomain(this);
        AddDomain(PlanetDomain);
        SocietyDomain = new SocietyDomain(this);
        AddDomain(SocietyDomain);
        
        LandSea = new LandSeaManager();
        Landforms = new LandformManager();
        Vegetation = new VegetationManager();
    }
}
