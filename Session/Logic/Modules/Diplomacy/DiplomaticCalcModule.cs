using System;
using System.Collections.Generic;
using System.Linq;

public class DiplomaticCalcModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        return;
    }
}
