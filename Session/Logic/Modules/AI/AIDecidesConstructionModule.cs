using System;
using System.Collections.Generic;
using System.Linq;

public class AIDecidesConstructionModule : LogicModule
{
    private EntityValueCache<Regime, RegimeAI> _ais;

    public AIDecidesConstructionModule(EntityValueCache<Regime, RegimeAI> ais)
    {
        _ais = ais;
    }
    public override void Calculate(Data data, 
        Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        foreach (var regime in data.Society.Regimes.Entities)
        {
            if (regime.IsPlayerRegime(data)) continue;
            _ais[regime].Construction.Calculate(data, queueMessage, queueEntityCreation);
        }
    }
}
