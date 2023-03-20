using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenAuxiliaryData 
{
    public Dictionary<MapPolygon, GenCell> PolyCells { get; private set; }
    public List<GenCell> Cells { get; private set; }
    public List<GenMass> Masses { get; private set; }
    public List<GenPlate> Plates { get; private set; }
    public List<GenContinent> Continents { get; private set; }
    public FaultLineManager FaultLines { get; private set; }
    private OpenSimplexNoise _altNoise;
    public GenAuxiliaryData(GenData data)
    {
        _altNoise = new OpenSimplexNoise();
        _altNoise.Period = data.GenSettings.Dimensions.x;
        _altNoise.Octaves = 3;
        _altNoise.Lacunarity = 2;
        _altNoise.Persistence = .5f;
        Cells = new List<GenCell>();
        PolyCells = new Dictionary<MapPolygon, GenCell>();
        Masses = new List<GenMass>();
        Plates = new List<GenPlate>();
        Continents = new List<GenContinent>();
        FaultLines = new FaultLineManager();
    }

    public float GetAltPerlin(Vector2 p)
    {
        return _altNoise.GetNoise2d(p.x, p.y);
    }
}