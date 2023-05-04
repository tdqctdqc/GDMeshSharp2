using System;
using System.Collections.Generic;
using System.Linq;

public class FarmerAttribute : PeepJobAttribute
{
    public FarmerAttribute() : base(PeepClassManager.Laborer)
    {
    }
}
