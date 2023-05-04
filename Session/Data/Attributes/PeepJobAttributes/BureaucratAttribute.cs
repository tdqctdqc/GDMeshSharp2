using System;
using System.Collections.Generic;
using System.Linq;

public class BureaucratAttribute : PeepJobAttribute
{
    public BureaucratAttribute() : base(PeepClassManager.Professional)
    {
    }
}
