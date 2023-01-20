using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenAuxiliaryData 
{
    
    public List<GenMass> Masses { get; private set; }
    public List<GenPlate> Plates { get; private set; }
    public List<GenContinent> Continents { get; private set; }
    public List<FaultLine> FaultLines { get; private set; }

    public GenAuxiliaryData()
    {
        Masses = new List<GenMass>();
        Plates = new List<GenPlate>();
        Continents = new List<GenContinent>();
        FaultLines = new List<FaultLine>();
    }
}