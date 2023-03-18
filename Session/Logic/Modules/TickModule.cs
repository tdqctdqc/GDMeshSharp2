
using System;
using System.Collections.Generic;
using System.Reflection;

public class TickModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queue)
    {
        queue(new TickProcedure());
    }
}
