using System;
using System.Collections.Generic;
using System.Linq;

public abstract class WorkBuildingModel : BuildingModel
{
    public abstract Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
    protected WorkBuildingModel(string name, float buildCost) : base(name, buildCost)
    {
    }
    public abstract void Produce(WorkProdConsumeProcedure proc, PolyTriPosition pos, float staffingRatio, Data data);
}
