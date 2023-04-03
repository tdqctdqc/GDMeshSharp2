using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : WorkBuildingModel
{
    public TownHall() : base(nameof(TownHall), 100f)
    {
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }

    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.BureaucratAttribute, 100}
        };
    public override void Produce(WorkProdConsumeProcedure proc, PolyTriPosition pos, float staffingRatio, Data data)
    {
        
    }
}
