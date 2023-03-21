using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DataNotices
{
    public RefAction FinishedStateSync { get; set; }
    public RefAction<Decision> NeedDecision { get; set; }
    public RefAction FinishedFrame { get; set; }
    public RefAction SetPolyShapes { get; set; }
    public DataNotices()
    {
        FinishedStateSync = new RefAction();
        NeedDecision = new RefAction<Decision>();
        FinishedFrame = new RefAction();
        SetPolyShapes = new RefAction();
    }
}

