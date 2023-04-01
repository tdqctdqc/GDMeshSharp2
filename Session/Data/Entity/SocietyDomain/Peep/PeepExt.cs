using System;
using System.Collections.Generic;
using System.Linq;

public static class PeepExt
{
    public static int GetUnemployedCount(this Peep peep)
    {
        return peep.Jobs[0].Count;
    }
}
