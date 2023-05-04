using System;
using System.Collections.Generic;
using System.Linq;

public class MinerAttribute : PeepJobAttribute
{
    public MinerAttribute() : base(PeepClassManager.Laborer)
    {
    }
}
