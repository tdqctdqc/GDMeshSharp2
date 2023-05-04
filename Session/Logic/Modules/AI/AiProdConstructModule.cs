using System;
using System.Collections.Generic;
using System.Linq;

public class AiProdConstructModule : LogicModule
{
    private EntityValueCache<Regime, RegimeAi> _ais;
    private float _stockpileRatio = .1f;
    public AiProdConstructModule(EntityValueCache<Regime, RegimeAi> ais, Data data)
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
