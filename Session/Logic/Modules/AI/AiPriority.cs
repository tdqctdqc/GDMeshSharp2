using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AiPriority
{
    private float _weight;
    public AiPriority(float weight)
    {
        _weight = weight;
    }

    protected abstract float GetDemand(Regime r, Data d);
    protected abstract float GetSupply(Regime r, Data d);
    public float GetUrgency(Regime r, Data data)
    {
        var demand = GetDemand(r, data);
        var supply = GetSupply(r, data);
        if (demand < supply) return 0f;
        return _weight * demand / supply;
    }

    public abstract void Calculate(Regime regime, Data data,
        ItemWallet budget,
        Dictionary<Item, float> prices,
        int credit,
        Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation);

}
