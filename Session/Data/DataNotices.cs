using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DataNotices
{
    public RefAction FinishedStateSync { get; private set; }
    public RefAction GeneratedRegimes { get; private set; }
    public RefAction PopulatedWorld { get; private set; }
    public RefAction MadeResources { get; private set; }
    public RefAction<Decision> NeedDecision { get; private set; }
    public RefAction<int> Ticked { get; private set; }
    public RefAction SetPolyShapes { get; private set; }
    public RefAction SetLandAndSea { get; private set; }
    public DataNotices()
    {
        PopulatedWorld = new RefAction();
        GeneratedRegimes = new RefAction();
        MadeResources = new RefAction();
        FinishedStateSync = new RefAction();
        NeedDecision = new RefAction<Decision>();
        Ticked = new RefAction<int>();
        SetPolyShapes = new RefAction();
        SetLandAndSea = new RefAction();
    }
}

