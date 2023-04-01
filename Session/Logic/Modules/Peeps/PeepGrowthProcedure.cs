using System;
using System.Collections.Generic;
using System.Linq;

public class PeepGrowthProcedure : Procedure
{
    public Dictionary<int, int> Growths { get; private set; }
    public PeepGrowthProcedure(Dictionary<int, int> growths)
    {
        Growths = growths;
    }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var kvp in Growths)
        {
            var peep = key.Data.Society.Peeps[kvp.Key];
            var growth = kvp.Value;
            if (growth < 0)
            {
                
            }

            if (growth > 0)
            {
                peep.GrowSize(growth, key);
            }
        }
    }
}
