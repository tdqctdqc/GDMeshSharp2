
using System;
using System.Collections.Generic;
using System.Reflection;

public class TickModule : LogicModule
{
    public override IResult Calculate(Data data)
    {
        return new TickResult();
    }
}

public class TickResult : IResult
{
    public void Poll(Action<Procedure> addProc, Action<Decision> addDec, Action<Update> addUpdate)
    {
        addProc(new TickProcedure());
    }
}
