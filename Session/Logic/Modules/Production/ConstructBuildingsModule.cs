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
                if (kvp.Value.TicksLeft < 0)
                {
                    finished.Add(kvp.Value);
                }
            }
        }
        foreach (var c in finished)
        {
            clear.Positions.Add(c.Pos);
            Func<HostWriteKey, Entity> create = k =>
            {
                return MapBuilding.Create(c.Pos, c.Model.Model(), k);
            };
            queueEntityCreation(create);
        }

        queueMessage(clear);
    }
}
