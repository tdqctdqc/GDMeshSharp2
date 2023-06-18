using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AiPriority
{
    public float Weight { get; private set; }
    public AiPriority(float weight)
    {
        Weight = weight;
    }

    protected abstract float GetDemand(Regime r, Data d);
    protected abstract float GetSupply(Regime r, Data d);
    public float GetPriorityWeight(Regime r, Data data)
    {
        var demand = GetDemand(r, data);
        var supply = GetSupply(r, data);
        if (demand < supply) return 1f;
        if (supply == 0f) return 1f;
        var ds = demand / supply;
        if (ds < 0f) throw new Exception();
        return Weight * demand / supply;
    }

    public abstract void Calculate(Regime regime, Data data,
        ItemWallet budget,
        Dictionary<Item, float> prices,
        int credit,
        Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation);

}
