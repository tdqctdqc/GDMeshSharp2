
using System.Collections.Generic;

public class TickModule : LogicModule
{
    public override List<Procedure> Calculate(Data data)
    {
        return new List<Procedure> {new TickProcedure()};
    }
}
