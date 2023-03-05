
using System.Collections.Generic;
using System.Reflection;

public class TickModule : LogicModule
{
    public override LogicResult Calculate(Data data)
    {
        return new LogicResult(new List<Procedure> {new PTick()}, 
            new List<Decision>(),
            new List<Update>()) ;
    }
}
