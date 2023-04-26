using System;
using System.Collections.Generic;
using System.Linq;

public class PeepHistory
{
    public CountHistory PeepCount { get; private set; }
    public CountHistory PeepSize { get; private set; }
    public CountHistory Unemployed { get; private set; }

    public static PeepHistory Construct()
    {
        return new PeepHistory(
            CountHistory.Construct(),
            CountHistory.Construct(),
            CountHistory.Construct()
            );
    }
    private PeepHistory(CountHistory peepCount,
        CountHistory peepSize,
        CountHistory unemployed)
    {
        PeepSize = peepSize;
        PeepCount = peepCount;
        Unemployed = unemployed;
    }
    public void Update(int tick, Regime regime, ProcedureWriteKey key)
    {
        var peeps = regime.GetPeeps(key.Data);
        var polys = regime.Polygons.Entities();
        PeepCount.Add(peeps.Count(), tick);
        PeepSize.Add(peeps.Sum(p => p.Size), tick);
        Unemployed.Add(polys.Select(p => p.Employment.NumUnemployed()).Sum(), tick);
    }
}
