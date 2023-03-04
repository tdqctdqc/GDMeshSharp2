using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeRepository : Repository<Regime>
{
    public RepoEntityMultiIndexer<Regime, MapPolygon> Territories { get; private set; } 
    public RegimeRepository(Domain domain, Data data) : base(domain, data)
    {
        Territories = new RepoEntityMultiIndexer<Regime, MapPolygon>(
            data, 
            p => p.Regime,
            nameof(MapPolygon.Regime)
        );
    }
}