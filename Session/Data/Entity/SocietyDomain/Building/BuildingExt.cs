using System;
using System.Collections.Generic;
using System.Linq;

public static class BuildingExt
{
    public static int NumWorkers(this Building b, Data data)
    {
        return b.Position.Poly(data).GetPeeps(data)
            .SelectMany(p => p.Jobs.Where(kvp => kvp.Value.Building.Entity() == b)).Sum(kvp => kvp.Value.Count);
    }
    public static int CalcNumWorkers(this Building b, Data data)
    {
        var peeps = b.Position.Poly(data)
            .GetPeeps(data);
        if (peeps == null) return 0;
        return peeps.SelectMany(p => p.Jobs.Values)
            .Where(ja => ja.Building.Entity() == b)
            .Sum(ja => ja.Count);
    }
}
