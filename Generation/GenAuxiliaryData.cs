using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenAuxiliaryData 
{
    public Dictionary<GenPolygon, GenCell> PolyCells { get; private set; }
    public List<GenCell> Cells { get; private set; }
    public List<GenMass> Masses { get; private set; }
    public List<GenPlate> Plates { get; private set; }
    public List<GenContinent> Continents { get; private set; }
    public List<FaultLine> FaultLines { get; private set; }

    public GenAuxiliaryData()
    {
        Cells = new List<GenCell>();
        PolyCells = new Dictionary<GenPolygon, GenCell>();
        Masses = new List<GenMass>();
        Plates = new List<GenPlate>();
        Continents = new List<GenContinent>();
        FaultLines = new List<FaultLine>();
    }
}