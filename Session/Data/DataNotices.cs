using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class DataNotices
{
    public RefAction FinishedStateSync { get; private set; }
    public RefAction GeneratedRegimes { get; private set; }
    public RefAction PopulatedWorld { get; private set; }
    public RefAction MadeResources { get; private set; }
    public RefAction<int> Ticked { get; private set; }
    public RefAction SetPolyShapes { get; private set; }
    public RefAction SetLandAndSea { get; private set; }
    public RefAction<Construction> StartedConstruction { get; private set; }
    public RefAction<Construction> EndedConstruction { get; private set; }
    
    public DataNotices()
    {
        PopulatedWorld = new RefAction();
        GeneratedRegimes = new RefAction();
        MadeResources = new RefAction();
        FinishedStateSync = new RefAction();
        Ticked = new RefAction<int>();
        SetPolyShapes = new RefAction();
        SetLandAndSea = new RefAction();
        StartedConstruction = new RefAction<Construction>();
        EndedConstruction = new RefAction<Construction>();
    }
}

