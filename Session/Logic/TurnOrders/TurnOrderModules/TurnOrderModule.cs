using System;
using System.Collections.Generic;
using System.Linq;

public abstract class TurnOrderModule
{
    public EntityRef<Regime> Regime { get; private set; }
    // public abstract void CalcForAi(Regime regime, Data data);
    public abstract void Enact(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation);
}
