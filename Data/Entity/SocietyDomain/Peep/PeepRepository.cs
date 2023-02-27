using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepRepository : Repository<Peep>
{
    public PeepRepository(Domain domain, Data data) : base(domain, data)
    {
    }
}