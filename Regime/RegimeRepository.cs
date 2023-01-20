using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeRepository : Repository<Regime>
{
    public RegimeRepository(Domain domain, Data data) : base(domain, data)
    {
    }
}