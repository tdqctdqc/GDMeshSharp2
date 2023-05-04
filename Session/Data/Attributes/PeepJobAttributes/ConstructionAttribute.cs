using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionAttribute : PeepJobAttribute
{
    public ConstructionAttribute() : base(PeepClassManager.Laborer)
    {
    }
}
