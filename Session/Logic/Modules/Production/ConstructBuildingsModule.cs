using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ConstructBuildingsModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var finished = new HashSet<Construction>();
        var clear = ClearFinishedConstructionsProcedure.Construct();
        foreach (var r in data.Society.Regimes.Entities)
        {
            foreach (var kvp in data.Society.CurrentConstruction.ByTri)
            {
                var progress = 100f;
                if (kvp.Value.TicksLeft < 0)
                {
                    finished.Add(kvp.Value);
                }
            }
        }
        foreach (var f in finished)
        {
            clear.Positions.Add(f.Pos);
            if (f.Pos.Tri(data).Landform == LandformManager.Urban)
            {
                var s = f.Pos.Poly(data).GetSettlement(data);
                var proc = new CreateSettlementBuildingProcedure(s.Id, f.Model);
                queueMessage(proc);
            }
            else
            {
                Func<HostWriteKey, Entity> create = k =>
                {
                    return MapBuilding.Create(f.Pos, f.Model.Model(), k);
                };
                queueEntityCreation(create);
            }
        }

        queueMessage(clear);
    }
}
