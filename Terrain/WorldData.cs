using Godot;
using System;
using System.Collections.Generic;

public class WorldData 
{
    public Vector2 Dimensions { get; private set; }
    public List<GeologyMass> Masses { get; private set; }
    public List<GeologyPlate> Plates { get; private set; }
    public List<GeologyCell> Cells { get; private set; }
    public List<GeologyPolygon> GeoPolygons { get; private set; }
    public List<Continent> Continents { get; private set; }
    public List<FaultLine> FaultLines { get; private set; }
    public WorldData(Vector2 dimensions)
    {
        Dimensions = dimensions;
        Masses = new List<GeologyMass>();
        Plates = new List<GeologyPlate>();
        Cells = new List<GeologyCell>();
        Continents = new List<Continent>();
        GeoPolygons = new List<GeologyPolygon>();
        FaultLines = new List<FaultLine>();
    }
}
