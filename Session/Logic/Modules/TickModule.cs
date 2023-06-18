
using System;
using System.Collections.Generic;
using System.Reflection;

public class TickModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        queueMessage(new TickProcedure());
    }

}
