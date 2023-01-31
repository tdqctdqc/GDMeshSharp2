using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepRepository : Repository<Peep>
{
    public PeepHomeAuxData Homes { get; private set; }
    public PeepRepository(Domain domain, Data data) : base(domain, data)
    {
        Homes = new PeepHomeAuxData(this);
    }
}