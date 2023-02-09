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
    
    public GenAuxiliaryData()
    {
        Cells = new List<GenCell>();
        PolyCells = new Dictionary<MapPolygon, GenCell>();
        Masses = new List<GenMass>();
        Plates = new List<GenPlate>();
        Continents = new List<GenContinent>();
        FaultLines = new FaultLineManager();
    }
}