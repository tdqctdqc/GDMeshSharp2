using System;
using System.Collections.Generic;
using System.Linq;

public class GathererAttribute : PeepJobAttribute
{
    public GathererAttribute() : base(PeepClassManager.Indigenous)
    {
    }
}
