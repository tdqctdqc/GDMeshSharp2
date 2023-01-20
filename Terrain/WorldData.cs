using Godot;
using System;
using System.Collections.Generic;

public class WorldData : Data
{
    public PlanetDomain PlanetDomain { get; private set; }
    public Vector2 Dimensions { get; private set; }
    public List<GenMass> Masses { get; private set; }
    public List<GenPlate> Plates { get; private set; }
    public List<GenContinent> Continents { get; private set; }
    public List<FaultLine> FaultLines { get; private set; }
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public LocationManager Locations { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public WorldData(Vector2 dimensions) : base()
    {
        Dimensions = dimensions;
        PlanetDomain = new PlanetDomain(this);
        AddDomain(PlanetDomain);
        Masses = new List<GenMass>();
        Plates = new List<GenPlate>();
        Continents = new List<GenContinent>();
        FaultLines = new List<FaultLine>();
        Locations = new LocationManager();
        LandSea = new LandSeaManager();
    }
    public void Setup(CreateWriteKey key, IDDispenser id)
    {
        Landforms = new LandformManager(key, id, this);
        Vegetation = new VegetationManager(key, id, this);
    }
}
