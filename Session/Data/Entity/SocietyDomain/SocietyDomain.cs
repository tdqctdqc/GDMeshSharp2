using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain : Domain
{
    public EntityRegister<Settlement> Settlements => GetRegister<Settlement>();
    public EntityRegister<RoadSegment> RoadSegments => GetRegister<RoadSegment>();
    public EntityRegister<Regime> Regimes => GetRegister<Regime>();
    public EntityRegister<Peep> Peeps => GetRegister<Peep>();
    public EntityRegister<Building> Buildings => GetRegister<Building>();
    public EntityRegister<RegimeRelation> RegimeRelations => GetRegister<RegimeRelation>();

    public SettlementAux SettlementAux { get; private set; }
    public RoadAux RoadAux { get; private set; }
    public RegimeAux RegimeAux { get; private set; }
    public PeepAux PeepAux { get; private set; }
    public RegimeRelationAux RelationAux { get; private set; }
    public BuildingAux BuildingAux { get; private set; }
    
    public SocietyDomain() : base(typeof(SocietyDomain))
    {
        
    }

    protected override void Setup()
    {
        SettlementAux = new SettlementAux(this, Data);
        RoadAux = new RoadAux(this, Data);
        RegimeAux = new RegimeAux(this, Data);
        PeepAux = new PeepAux(this, Data);
        RelationAux = new RegimeRelationAux(this, Data);
        BuildingAux = new BuildingAux(this, Data);
        
        AddRepo(SettlementAux);
        AddRepo(RoadAux);
        AddRepo(RegimeAux);
        AddRepo(PeepAux);
        AddRepo(RelationAux);
        AddRepo(BuildingAux);
    }
}