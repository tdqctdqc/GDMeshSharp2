using System;
using System.Collections.Generic;
using System.Linq;

public class ProleAttribute : PeepJobAttribute
{
    public ProleAttribute() : base(PeepClassManager.Laborer)
    {
    }
}