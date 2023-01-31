using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeRepository : Repository<Regime>
{
    public Dictionary<Regime, RegimeTerritory> Territories { get; private set; }
    public RegimeRepository(Domain domain, Data data) : base(domain, data)
    {
        Territories = new Dictionary<Regime, RegimeTerritory>();
        
        data.Notices.RegisterEntityAddedCallback<Regime>(
            regime =>
            {
                Territories.Add(regime, new RegimeTerritory(regime, data));
            }
        );
        data.Notices.RegisterEntityRemovingCallback<Regime>(
            regime =>
            {
                Territories.Remove(regime);
            }
        );
    }
}