using System;
using System.Collections.Generic;
using System.Linq;

public abstract class WorkBuildingModel : BuildingModel
{
    public abstract Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
    protected WorkBuildingModel(string name, int numTicksToBuild, int laborPerTickToBuild)
        : base(name, numTicksToBuild, laborPerTickToBuild)
    {
    }
    public abstract void Produce(WorkProdConsumeProcedure proc, PolyTriPosition pos, float staffingRatio, int ticksSinceLast,
        Data data);

    public int TotalLaborReq()
    {
        return JobLaborReqs.Sum(kvp => kvp.Value);
    }
}
