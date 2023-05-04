using System;
using System.Collections.Generic;
using System.Linq;

public static class PolyPeepExt
{
    public static int GetNumOfClass(this PolyPeep peep, PeepClass peepClass)
    {
        if (peep.ClassFragments.ContainsKey(peepClass.Name))
        {
            return peep.ClassFragments[peepClass.Name].Size;
        }

        return 0;
    }    
}
