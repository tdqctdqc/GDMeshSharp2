using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : WorkBuildingModel
{
    public override int Capacity { get; } = 100;

    public override Dictionary<PeepJobAttribute, int> JobLaborReqs { get; }
        = new Dictionary<PeepJobAttribute, int>
        {
            {PeepJobAttribute.BureaucratAttribute, 100}
        };
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 200}
        };
    public TownHall() : base(BuildingType.Government, nameof(TownHall), 50, 100)
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

    
    public override void Produce(WorkProdConsumeProcedure proc, MapPolygon poly, float staffingRatio,
        int ticksSinceLast, Data data)
    {
        
    }
    
}
