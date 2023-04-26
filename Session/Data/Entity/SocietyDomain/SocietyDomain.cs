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
    public EntityRegister<PolyPeeps> PolyPeeps => GetRegister<PolyPeeps>();
    public EntityRegister<MapBuilding> Buildings => GetRegister<MapBuilding>();
    public EntityRegister<RegimeRelation> RegimeRelations => GetRegister<RegimeRelation>();
    public SettlementAux SettlementAux { get; private set; }
    public RoadAux RoadAux { get; private set; }
    public RegimeAux RegimeAux { get; private set; }
    public PeepAux PeepAux { get; private set; }
    public RegimeRelationAux RelationAux { get; private set; }
    public BuildingAux BuildingAux { get; private set; }
    public CurrentConstruction CurrentConstruction => _construction.Value;
    private SingletonAux<CurrentConstruction> _construction;
    public SocietyDomain(Data data) : base(typeof(SocietyDomain), data)
    {
        
    }
    public override void Setup()
    {
        SettlementAux = new SettlementAux(this, Data);
        RoadAux = new RoadAux(this, Data);
        RegimeAux = new RegimeAux(this, Data);
        PeepAux = new PeepAux(this, Data);
        RelationAux = new RegimeRelationAux(this, Data);
        BuildingAux = new BuildingAux(this, Data);
        _construction = new SingletonAux<CurrentConstruction>(this, Data);
    }
}