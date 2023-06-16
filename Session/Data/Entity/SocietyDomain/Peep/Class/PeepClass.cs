using System;
using System.Collections.Generic;
using System.Linq;

public class PeepClass : IModel
{
    public string Name { get; }
    public int Id { get; private set; }
    public PeepClass(string name)
    {
        Name = name;
    }

    

}
