using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DataNotices
{
    public RefAction FinishedStateSync { get; set; }
    public RefAction<Decision> NeedDecision { get; set; }
    public RefAction Ticked { get; set; }
    public RefAction SetPolyShapes { get; set; }
    public RefAction SetLandAndSea { get; set; }
    public DataNotices()
    {
        FinishedStateSync = new RefAction();
        NeedDecision = new RefAction<Decision>();
        Ticked = new RefAction();
        SetPolyShapes = new RefAction();
        SetLandAndSea = new RefAction();
    }
}

