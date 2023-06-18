using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

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
        Parallel.ForEach(data.Society.Regimes.Entities, regime =>
        {
            if (regime.IsPlayerRegime(data)) return;
            _ais[regime].Budget.Calculate(data, queueMessage, queueEntityCreation);
        });
    }
}
