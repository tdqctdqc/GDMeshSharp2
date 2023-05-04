using System;
using System.Collections.Generic;
using System.Linq;

public class PeepClass : IModel
{
    public PeepClass(string name)
    {
        Name = name;
    }

    public string Name { get; }
    
}
