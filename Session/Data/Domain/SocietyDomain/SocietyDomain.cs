using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain : Domain
{
    public Repository<Settlement> Settlements { get; private set; }
    public RoadRepository Roads { get; private set; }
    public RegimeRepository Regimes { get; private set; }
    public SocietyDomain(Data data)
    {
        Settlements = new Repository<Settlement>(this, data);
        AddRepo(Settlements);

        Roads = new RoadRepository(this, data);
        AddRepo(Roads);

        Regimes = new RegimeRepository(this, data);
        AddRepo(Regimes);
    }
}