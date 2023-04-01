using System;
using System.Collections.Generic;
using System.Linq;

public abstract class WorkBuildingModel : BuildingModel
{
    public abstract int PeepsLaborReq { get; }
    public abstract PeepJob JobType { get; }
    protected WorkBuildingModel(string name, float buildCost) : base(name, buildCost)
    {
    }
}
