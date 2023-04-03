using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var progresses = new Dictionary<Construction, float>();
        var finished = new HashSet<Construction>();
        foreach (var r in data.Society.Regimes.Entities)
        {
            foreach (var construction in r.CurrentConstruction.Constructions)
            {
                var progress = 100f;
                if (construction.Value.Progress + progress >= construction.Value.Model.Model().BuildCost)
                {
                    finished.Add(construction.Value);
                }
                else
                {
                    progresses.Add(construction.Value, progress);
                }
            }
        }
        foreach (var f in finished)
        {
            Func<HostWriteKey, Entity> create = k =>
            {
                return MapBuilding.Create(f.Pos, f.Model.Model(), k);
            };
            queueEntityCreation(create);
        }
    }
}
