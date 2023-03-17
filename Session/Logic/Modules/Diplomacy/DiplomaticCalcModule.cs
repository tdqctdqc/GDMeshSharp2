using System.Collections.Generic;
using System.Linq;

public class DiplomaticCalcModule : LogicModule
{
    public override IResult Calculate(Data data)
    {
        var regimes = data.Society.Regimes.Entities;
        var r0 = regimes.ElementAt(0);
        var r1 = regimes.ElementAt(1);
        return null;
    }
}
