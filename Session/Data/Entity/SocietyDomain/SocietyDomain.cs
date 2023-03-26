using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain : Domain
{
    public EntityRegister<Settlement> SettlementR => GetRegister<Settlement>();
    public EntityRegister<RoadSegment> RoadSegmentR => GetRegister<RoadSegment>();
    public EntityRegister<Regime> RegimeR => GetRegister<Regime>();
    public EntityRegister<Peep> PeepR => GetRegister<Peep>();
    public EntityRegister<Building> BuildingR => GetRegister<Building>();
    public EntityRegister<RegimeRelation> RegimeRelationR => GetRegister<RegimeRelation>();

    public SettlementRepository Settlements { get; private set; }
    public RoadRepository Roads { get; private set; }
    public RegimeRepository Regimes { get; private set; }
    public PeepRepository Peeps { get; private set; }
    public RegimeRelationRepo Relations { get; private set; }
    public BuildingRepo Buildings { get; private set; }
    
    public SocietyDomain(Data data) : base(data, typeof(BaseDomain))
    {
        Settlements = new SettlementRepository(this, data);
        AddRepo(Settlements);

        Roads = new RoadRepository(this, data);
        AddRepo(Roads);

        Regimes = new RegimeRepository(this, data);
        AddRepo(Regimes);

        Peeps = new PeepRepository(this, data);
        AddRepo(Peeps);

        Relations = new RegimeRelationRepo(this, data);
        AddRepo(Relations);
        Buildings = new BuildingRepo(this, data);
        AddRepo(Buildings);
    }
}