using System;
using System.Collections.Generic;
using System.Linq;

public static class PeepExt
{
    public static int GetUnemployedCount(this Peep peep)
    {
        return peep.Jobs[PeepJobManager.Unemployed].Count;
    }
}
