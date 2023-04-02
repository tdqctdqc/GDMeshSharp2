using System;
using System.Collections.Generic;
using System.Linq;

public class PeepGrowthAndDeclineProcedure : Procedure
{
    public Dictionary<int, int> Growths { get; private set; }
    public PeepGrowthAndDeclineProcedure(Dictionary<int, int> growths)
    {
        Growths = growths;
    }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        DoGrowth(key);
        DoHistory(key);
    }

    private void DoGrowth(ProcedureWriteKey key)
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
    private void DoHistory(ProcedureWriteKey key)
    {
        var tick = key.Data.BaseDomain.GameClock.Tick;
        foreach (var r in key.Data.Society.Regimes.Entities)
        {
            var peepCount = r.GetPeeps(key.Data).Count();
            r.PeepHistory.Update(tick, peepCount, key);
        }
    }
}
