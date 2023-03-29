using System;
using System.Collections.Generic;
using System.Linq;

public class Construction
{
    public ModelRef<BuildingModel> Model { get; private set; }
    public float Progress { get; private set; }

    public bool ProgressConstruction(float progress)
    {
        Progress += progress;
        return Progress >= Model.Model().BuildCost;
    }
}
