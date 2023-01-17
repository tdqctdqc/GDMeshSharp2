using Godot;
using System;
using System.Collections.Generic;

public class WorldData 
{
    public Vector2 Dimensions { get; private set; }
    public List<GeoMass> Masses { get; private set; }
    public List<GeoPlate> Plates { get; private set; }
    public List<GeoCell> Cells { get; private set; }
    public List<GeoPolygon> GeoPolygons { get; private set; }
    public List<Continent> Continents { get; private set; }
    public List<FaultLine> FaultLines { get; private set; }
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public LocationManager Locations { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public WorldData(Vector2 dimensions)
    {
        Dimensions = dimensions;
        Masses = new List<GeoMass>();
        Plates = new List<GeoPlate>();
        Cells = new List<GeoCell>();
        Continents = new List<Continent>();
        GeoPolygons = new List<GeoPolygon>();
        FaultLines = new List<FaultLine>();
        Landforms = new LandformManager();
        Vegetation = new VegetationManager();
        Locations = new LocationManager();
        LandSea = new LandSeaManager();
    }
}
